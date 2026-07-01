import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { userSettingsQueryKey } from "~/helpers/requests";
import {
  ActionIcon,
  LoadingOverlay,
  Stack,
  Popover as MantinePopover,
  Group,
} from "@mantine/core";
import { useField } from "@mantine/form";
import { ICategory } from "~/models/category";
import { useQuery } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
import { PlusIcon, SendIcon } from "lucide-react";
import React from "react";
import { IUserSettings } from "~/models/userSettings";
import { getCurrencySymbol } from "~/helpers/currency";
import Popover from "~/components/core/Popover/Popover";
import CategorySelect from "~/components/core/Select/CategorySelect/CategorySelect";
import NumberInput from "~/components/core/Input/NumberInput/NumberInput";
import { useTranslation } from "react-i18next";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import { useCreateBudgetMutation } from "~/hooks/mutations/budgets/useCreateBudgetMutation";

interface AddBudgetProps {
  date: Date;
  categories: ICategory[];
}

const AddBudget = (props: AddBudgetProps): React.ReactNode => {
  const { t } = useTranslation();
  const { thousandsSeparator, decimalSeparator, dayjs } = useLocale();
  const createBudgetMutation = useCreateBudgetMutation();

  const categoryField = useField<string>({
    initialValue: "",
  });
  const limitField = useField<string | number>({
    initialValue: "",
  });

  const { request } = useAuth();

  const userSettingsQuery = useQuery({
    queryKey: [userSettingsQueryKey],
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

  return (
    <Popover>
      <MantinePopover.Target>
        <ActionIcon size="input-sm">
          <PlusIcon />
        </ActionIcon>
      </MantinePopover.Target>
      <MantinePopover.Dropdown p="0.5rem">
        <LoadingOverlay visible={createBudgetMutation.isPending} />
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
                createBudgetMutation.mutate([
                  {
                    month: dayjs(props.date).format("YYYY-MM-DD"),
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
