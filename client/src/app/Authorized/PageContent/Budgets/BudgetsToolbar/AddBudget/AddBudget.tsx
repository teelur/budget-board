import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { translateAxiosError } from "~/helpers/requests";
import {
  ActionIcon,
  LoadingOverlay,
  Stack,
  Popover as MantinePopover,
  Group,
} from "@mantine/core";
import { useField } from "@mantine/form";
import { notifications } from "@mantine/notifications";
import { IBudgetCreateRequest } from "~/models/budget";
import { ICategory } from "~/models/category";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { AxiosError, AxiosResponse } from "axios";
import { PlusIcon, SendIcon } from "lucide-react";
import React from "react";
import { IUserSettings } from "~/models/userSettings";
import { getCurrencySymbol } from "~/helpers/currency";
import Popover from "~/components/core/Popover/Popover";
import CategorySelect from "~/components/core/Select/CategorySelect/CategorySelect";
import NumberInput from "~/components/core/Input/NumberInput/NumberInput";
import { useTranslation } from "react-i18next";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";

interface AddBudgetProps {
  date: Date;
  categories: ICategory[];
}

const AddBudget = (props: AddBudgetProps): React.ReactNode => {
  const { t } = useTranslation();
  const { thousandsSeparator, decimalSeparator } = useLocale();

  const categoryField = useField<string>({
    initialValue: "",
  });
  const limitField = useField<string | number>({
    initialValue: "",
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
  const doCreateBudget = useMutation({
    mutationFn: async (newBudget: IBudgetCreateRequest[]) =>
      await request({
        url: "/api/budget",
        method: "POST",
        data: newBudget,
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["budgets"] });
    },
    onError: (error: AxiosError) => {
      notifications.show({
        message: translateAxiosError(error),
        color: "var(--button-color-destructive)",
      });
    },
  });

  return (
    <Popover>
      <MantinePopover.Target>
        <ActionIcon size="input-sm">
          <PlusIcon />
        </ActionIcon>
      </MantinePopover.Target>
      <MantinePopover.Dropdown p="0.5rem">
        <LoadingOverlay visible={doCreateBudget.isPending} />
        <Group gap="0.5rem">
          <Stack gap="0.5rem">
            <CategorySelect
              {...categoryField.getInputProps()}
              categories={props.categories}
              elevation={1}
            />
            <NumberInput
              {...limitField.getInputProps()}
              placeholder={t("limit")}
              w="100%"
              prefix={getCurrencySymbol(userSettingsQuery.data?.currency)}
              min={0}
              decimalScale={2}
              thousandSeparator={thousandsSeparator}
              decimalSeparator={decimalSeparator}
              elevation={1}
            />
          </Stack>
          <Stack
            style={{
              alignSelf: "stretch",
            }}
          >
            <ActionIcon
              h="100%"
              disabled={
                categoryField.getValue() === "" || limitField.getValue() === ""
              }
              onClick={() =>
                doCreateBudget.mutate([
                  {
                    date: props.date,
                    category: categoryField.getValue(),
                    limit:
                      limitField.getValue() === ""
                        ? 0
                        : (limitField.getValue() as number),
                  },
                ])
              }
            >
              <SendIcon size={18} />
            </ActionIcon>
          </Stack>
        </Group>
      </MantinePopover.Dropdown>
    </Popover>
  );
};

export default AddBudget;
