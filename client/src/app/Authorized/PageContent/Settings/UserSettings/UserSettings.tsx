import { LoadingOverlay, Skeleton, Stack } from "@mantine/core";
import { useField } from "@mantine/form";
import { notifications } from "@mantine/notifications";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
import React from "react";
import { Trans, useTranslation } from "react-i18next";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { translateAxiosError } from "~/helpers/requests";
import {
  DateFormatItem,
  DateFormats,
  IUserSettings,
  IUserSettingsUpdateRequest,
  LanguageItem,
  Languages,
} from "~/models/userSettings";
import Card from "~/components/core/Card/Card";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import Select from "~/components/core/Select/Select/Select";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";

const UserSettings = (): React.ReactNode => {
  const currencyField = useField({
    initialValue: "",
  });
  const languageField = useField({
    initialValue: "",
  });
  const dateFormatField = useField({
    initialValue: "",
  });

  const { t } = useTranslation();
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
  const doUpdateUserSettings = useMutation({
    mutationFn: async (updatedUserSettings: IUserSettingsUpdateRequest) =>
      await request({
        url: "/api/userSettings",
        method: "PUT",
        data: updatedUserSettings,
      }),
    onSuccess: async () => {
      // Need to explicitly refetch to get updated settings
      await queryClient.refetchQueries({ queryKey: ["userSettings"] });
    },
    onError: (error: any) => {
      notifications.show({
        color: "var(--button-color-destructive)",
        message: translateAxiosError(error),
      });
    },
  });

  React.useEffect(() => {
    if (userSettingsQuery.data?.currency) {
      currencyField.setValue(userSettingsQuery.data.currency);
    }
    if (userSettingsQuery.data?.language) {
      languageField.setValue(userSettingsQuery.data.language);
    }
    if (userSettingsQuery.data?.dateFormat) {
      dateFormatField.setValue(userSettingsQuery.data.dateFormat);
    }
  }, [
    userSettingsQuery.data?.currency,
    userSettingsQuery.data?.language,
    userSettingsQuery.data?.dateFormat,
    currencyField.setValue,
    languageField.setValue,
    dateFormatField.setValue,
  ]);

  return (
    <Card elevation={1}>
      <LoadingOverlay visible={doUpdateUserSettings.isPending} />
      <Stack gap="0.5rem">
        <PrimaryText size="lg">{t("user_settings")}</PrimaryText>
        <Stack gap="0.5rem">
          {userSettingsQuery.isPending ? (
            <Skeleton h={60} radius="md" />
          ) : (
            <Stack gap="0.25rem">
              <Select
                label={
                  <PrimaryText size="sm">{t("preferred_currency")}</PrimaryText>
                }
                placeholder={t("select_currency")}
                searchable
                nothingFoundMessage={t("no_currencies_found")}
                data={Intl.supportedValuesOf("currency")}
                {...currencyField.getInputProps()}
                onChange={(value) => {
                  if (value) {
                    doUpdateUserSettings.mutate({
                      currency: value,
                    });
                  }
                }}
                elevation={1}
              />
              <Select
                label={
                  <Stack gap="0">
                    <PrimaryText size="sm">
                      {t("preferred_language")}
                    </PrimaryText>
                    <DimmedText size="xs">
                      <Trans
                        i18nKey="preferred_language_description"
                        components={[
                          <a
                            key="link"
                            href="https://hosted.weblate.org/engage/budget-board/"
                            target="_blank"
                            rel="noopener noreferrer"
                            style={{
                              color: "inherit",
                              textDecoration: "underline",
                            }}
                          />,
                        ]}
                      />
                    </DimmedText>
                  </Stack>
                }
                placeholder={t("select_your_preferred_language")}
                data={Languages.map((lang: LanguageItem) => ({
                  value: lang.value,
                  label: t(lang.label),
                }))}
                {...languageField.getInputProps()}
                onChange={(value) => {
                  if (value) {
                    doUpdateUserSettings.mutate({
                      language: value,
                    });
                  }
                }}
                elevation={1}
              />
              <Select
                label={
                  <PrimaryText size="sm">
                    {t("preferred_date_format")}
                  </PrimaryText>
                }
                placeholder={t("select_your_preferred_date_format")}
                data={DateFormats.map((dateFormatItem: DateFormatItem) => ({
                  value: dateFormatItem.value,
                  label: t(dateFormatItem.label),
                }))}
                {...dateFormatField.getInputProps()}
                onChange={(value) => {
                  if (value) {
                    doUpdateUserSettings.mutate({
                      dateFormat: value,
                    });
                  }
                }}
                elevation={1}
              />
            </Stack>
          )}
        </Stack>
      </Stack>
    </Card>
  );
};

export default UserSettings;
