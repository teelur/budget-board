import { Button, Stack } from "@mantine/core";
import { useField } from "@mantine/form";
import { useDisclosure } from "@mantine/hooks";
import { notifications } from "@mantine/notifications";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { AxiosError, AxiosResponse } from "axios";
import React from "react";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { translateAxiosError } from "~/helpers/requests";
import Modal from "~/components/core/Modal/Modal";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import DateInput from "~/components/core/Input/DateInput/DateInput";
import { useTranslation } from "react-i18next";
import { ITrainAutoCategorizer as ITrainAutoCategorizerRequest } from "~/models/autoCategorizer";
import { IUserSettings } from "~/models/userSettings";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";

const TrainAutoCategorizerModal = (): React.ReactNode => {
  const [opened, { open, close }] = useDisclosure(false);

  const { t } = useTranslation();

  const startDateField = useField<Date | null>({
    initialValue: null,
  });
  const endDateField = useField<Date | null>({
    initialValue: null,
  });

  const { request } = useAuth();

  const userSettingsQuery = useQuery({
    queryKey: ["userSettings"],
    queryFn: async (): Promise<IUserSettings | undefined> => {
      const res: AxiosResponse = await request({
        url: "/api/userSettings",
        method: "GET",
      });

      if (res.status === 200) {
        return res.data as IUserSettings;
      }

      return undefined;
    },
  });

  const queryClient = useQueryClient();
  const doTrainAutoCategorizer = useMutation({
    mutationFn: async (trainAutoCategorizer: ITrainAutoCategorizerRequest) =>
      await request({
        url: "/api/trainAutoCategorizer",
        method: "POST",
        data: trainAutoCategorizer,
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["userSettings"] });
    },
    onError: (error: AxiosError) => {
      notifications.show({
        message: translateAxiosError(error),
        color: "var(--button-color-destructive)",
      });
    },
  });

  const onSubmit = () => {
    const startDate = startDateField.getValue();
    const endDate = endDateField.getValue();
    if (
      startDate != null &&
      endDate != null &&
      startDate > endDate
    ) {
      notifications.show({
              color: "var(--button-color-destructive)",
              message: t("train_auto_categorizer_dates_error"),
            });
      return;
    }

    doTrainAutoCategorizer.mutate({
      startDate: startDateField.getValue(),
      endDate: endDateField.getValue(),
    } as ITrainAutoCategorizerRequest);
  };

  return (
    <>
      <PrimaryText size="sm">
        {t("train_auto_categorizer")}
      </PrimaryText>
      <DimmedText size="xs">
        {t("train_auto_categorizer_description")}
      </DimmedText>
      <DimmedText size="xs">
        { userSettingsQuery.data?.autoCategorizerLastTrained != null
            ? t("train_auto_categorizer_last_trained", {
              lastTrained: userSettingsQuery.data?.autoCategorizerLastTrained,
              trainDataStartDate: userSettingsQuery.data?.autoCategorizerModelStartDate,
              trainDataEndDate: userSettingsQuery.data?.autoCategorizerModelEndDate
            })
            : t("train_auto_categorizer_not_trained")
        }
      </DimmedText>
      <Button size="input-sm" onClick={open}>
        {t("train_auto_categorizer_button")}
      </Button>
      <Modal
        opened={opened}
        onClose={close}
        title={<PrimaryText>{t("train_auto_categorizer")}</PrimaryText>}
      >
        <Stack gap="0.25rem">
          <DateInput
            label={<PrimaryText size="sm">{t("start_date")}</PrimaryText>}
            placeholder={t("select_a_date")}
            {...startDateField.getInputProps()}
            elevation={0}
            clearable
          />
          <DateInput
            label={<PrimaryText size="sm">{t("end_date")}</PrimaryText>}
            placeholder={t("select_a_date")}
            {...endDateField.getInputProps()}
            elevation={0}
            clearable
          />
          <Button
            mt="0.25rem"
            onClick={onSubmit}
            loading={doTrainAutoCategorizer.isPending}
          >
            {t("submit")}
          </Button>
        </Stack>
      </Modal>
    </>
  );
};

export default TrainAutoCategorizerModal;
