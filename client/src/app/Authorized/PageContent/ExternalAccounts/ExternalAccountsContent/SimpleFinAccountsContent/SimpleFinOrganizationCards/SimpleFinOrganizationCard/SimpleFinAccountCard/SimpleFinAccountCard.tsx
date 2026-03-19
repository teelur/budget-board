import { ActionIcon, Badge, Group, LoadingOverlay, Stack } from "@mantine/core";
import { DateValue } from "@mantine/dates";
import { useField } from "@mantine/form";
import { useDisclosure } from "@mantine/hooks";
import { notifications } from "@mantine/notifications";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
import { PencilIcon } from "lucide-react";
import React from "react";
import { Trans, useTranslation } from "react-i18next";
import Card from "~/components/core/Card/Card";
import DateInput from "~/components/core/Input/DateInput/DateInput";
import Select from "~/components/core/Select/Select/Select";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import StatusText from "~/components/core/Text/StatusText/StatusText";
import { convertNumberToCurrency, SignDisplay } from "~/helpers/currency";
import {
  simpleFinAccountQueryKey,
  simpleFinOrganizationQueryKey,
  translateAxiosError,
} from "~/helpers/requests";
import { AccountSource, IAccountResponse } from "~/models/account";
import { ISimpleFinAccountResponse } from "~/models/simpleFinAccount";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";

interface ISimpleFinAccountCardProps {
  simpleFinAccount: ISimpleFinAccountResponse;
}

const SimpleFinAccountCard = (
  props: ISimpleFinAccountCardProps,
): React.ReactNode => {
  const [isEditable, { toggle }] = useDisclosure(false);

  const linkedAccountIdField = useField<string[]>({
    initialValue: props.simpleFinAccount.linkedAccountId
      ? [props.simpleFinAccount.linkedAccountId]
      : [],
  });
  const syncStartDateField = useField<DateValue>({
    initialValue: props.simpleFinAccount.syncStartDate
      ? new Date(props.simpleFinAccount.syncStartDate)
      : null,
  });

  const { t } = useTranslation();
  const { dayjs, dateFormat, intlLocale, dayjsLocale } = useLocale();
  const { request } = useAuth();

  const accountsQuery = useQuery({
    queryKey: ["accounts"],
    queryFn: async (): Promise<IAccountResponse[]> => {
      const res: AxiosResponse = await request({
        url: "/api/account",
        method: "GET",
      });

      if (res.status === 200) {
        return res.data as IAccountResponse[];
      }

      return [];
    },
  });

  const simpleFinAccountsQuery = useQuery({
    queryKey: [simpleFinAccountQueryKey],
    queryFn: async (): Promise<ISimpleFinAccountResponse[]> => {
      const res: AxiosResponse = await request({
        url: "/api/simpleFinAccount",
        method: "GET",
      });

      if (res.status === 200) {
        return res.data as ISimpleFinAccountResponse[];
      }

      return [];
    },
  });

  const queryClient = useQueryClient();
  const doUpdateLinkedAccount = useMutation({
    mutationFn: async (updateLinkedAccountRequest: {
      simpleFinAccountGuid: string;
      linkedAccountGuid: string | null;
    }) =>
      await request({
        url: "/api/simpleFinAccount/updateLinkedAccount",
        method: "PUT",
        params: {
          simpleFinAccountGuid: updateLinkedAccountRequest.simpleFinAccountGuid,
          linkedAccountGuid: updateLinkedAccountRequest.linkedAccountGuid,
        },
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: [simpleFinOrganizationQueryKey],
      });
      queryClient.invalidateQueries({ queryKey: [simpleFinAccountQueryKey] });
      queryClient.invalidateQueries({ queryKey: ["institutions"] });
      queryClient.invalidateQueries({ queryKey: ["accounts"] });
    },
    onError: (error: any) => {
      notifications.show({
        color: "var(--button-color-destructive)",
        message: translateAxiosError(error),
      });
    },
  });
  const doUpdateSyncStartDate = useMutation({
    mutationFn: async (updateSyncStartDateRequest: {
      simpleFinAccountGuid: string;
      syncStartDate: Date | null;
    }) =>
      await request({
        url: "/api/simpleFinAccount/updateSyncStartDate",
        method: "PUT",
        params: {
          simpleFinAccountGuid: updateSyncStartDateRequest.simpleFinAccountGuid,
          syncStartDate: dayjs(
            updateSyncStartDateRequest.syncStartDate,
          ).isValid()
            ? dayjs(updateSyncStartDateRequest.syncStartDate).format(
                "YYYY-MM-DD",
              )
            : null,
        },
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [simpleFinAccountQueryKey] });
      queryClient.invalidateQueries({
        queryKey: [simpleFinOrganizationQueryKey],
      });
    },
    onError: (error: any) => {
      notifications.show({
        color: "var(--button-color-destructive)",
        message: translateAxiosError(error),
      });
    },
  });

  const getAccountNameForId = (accountId: string): string => {
    const account = accountsQuery.data?.find(
      (account) => account.id === accountId,
    );
    return account ? account.name : t("unknown_account");
  };

  const isLinkedAccountDeleted = React.useMemo(() => {
    if (!props.simpleFinAccount.linkedAccountId) {
      return false;
    }
    const linkedAccount = accountsQuery.data?.find(
      (account) => account.id === props.simpleFinAccount.linkedAccountId,
    );
    return linkedAccount?.deleted != null;
  }, [accountsQuery.data, props.simpleFinAccount.linkedAccountId]);

  React.useEffect(() => {
    linkedAccountIdField.setValue(
      props.simpleFinAccount.linkedAccountId != null &&
        props.simpleFinAccount.linkedAccountId.length > 0
        ? [props.simpleFinAccount.linkedAccountId]
        : [],
    );
  }, [props.simpleFinAccount.linkedAccountId]);

  React.useEffect(() => {
    syncStartDateField.setValue(
      dayjs(props.simpleFinAccount.syncStartDate).isValid()
        ? dayjs(props.simpleFinAccount.syncStartDate).toDate()
        : null,
    );
  }, [props.simpleFinAccount.syncStartDate]);

  const getBadgeForAccountName = (): React.ReactElement => {
    return props.simpleFinAccount.linkedAccountId ? (
      <Badge key="value" size="sm" />
    ) : (
      <Badge key="value" size="sm" color="gray" />
    );
  };

  const getBadgeForSyncStartDate = (): React.ReactElement => {
    return props.simpleFinAccount.syncStartDate ? (
      <Badge key="value" size="sm" color="var(--accent-color-purple)" />
    ) : (
      <Badge key="value" size="sm" color="gray" />
    );
  };

  const selectableAccounts = React.useMemo(() => {
    const linkedAccountIds = simpleFinAccountsQuery.data?.map(
      (sfa) => sfa.linkedAccountId,
    );

    return accountsQuery.data
      ?.filter(
        (account) =>
          (!linkedAccountIds?.includes(account.id) &&
            account.deleted == null &&
            account.source !== AccountSource.LunchFlow) ||
          account.id === props.simpleFinAccount.linkedAccountId,
      )
      .map((account) => ({
        value: account.id,
        label: account.name,
      }));
  }, [
    accountsQuery.data,
    simpleFinAccountsQuery.data,
    props.simpleFinAccount.linkedAccountId,
  ]);

  const accountCurrency = React.useMemo(() => {
    const accountCurrency = props.simpleFinAccount.currency;
    // Check if the currency is a valid ISO 4217 currency code
    const iso4217CurrencyCodes = Intl.NumberFormat.supportedLocalesOf([
      "en",
    ]).map((locale) => {
      const formatter = new Intl.NumberFormat(locale, {
        style: "currency",
        currency:
          accountCurrency != null && accountCurrency.length > 0
            ? accountCurrency
            : "USD",
      });
      const parts = formatter.formatToParts(1);
      const currencyPart = parts.find((part) => part.type === "currency");
      return currencyPart ? currencyPart.value : null;
    });

    return iso4217CurrencyCodes.includes(accountCurrency ?? "")
      ? accountCurrency
      : "USD";
  }, [accountsQuery.data, props.simpleFinAccount.linkedAccountId]);

  return (
    <Card elevation={2}>
      <LoadingOverlay
        visible={
          doUpdateLinkedAccount.isPending || doUpdateSyncStartDate.isPending
        }
      />
      <Stack gap={0}>
        <Group justify="space-between" align="center">
          <Group gap="0.5rem">
            <PrimaryText size="sm">{props.simpleFinAccount.name}</PrimaryText>
            <ActionIcon
              variant={isEditable ? "outline" : "transparent"}
              size="md"
              onClick={(e) => {
                e.stopPropagation();
                toggle();
              }}
            >
              <PencilIcon size={16} />
            </ActionIcon>
          </Group>
          <StatusText size="sm" amount={props.simpleFinAccount.balance}>
            {convertNumberToCurrency(
              props.simpleFinAccount.balance,
              true,
              accountCurrency,
              SignDisplay.Auto,
              intlLocale,
            )}
          </StatusText>
        </Group>
        <Group justify="space-between" align="center">
          <Group gap="0.5rem">
            {isEditable ? (
              <Group gap="0.5rem">
                <PrimaryText size="xs">{t("linked_account_input")}</PrimaryText>
                <Select
                  size="xs"
                  placeholder={t("select_an_account")}
                  data={selectableAccounts}
                  value={props.simpleFinAccount.linkedAccountId}
                  onChange={(value) => {
                    doUpdateLinkedAccount.mutate({
                      simpleFinAccountGuid: props.simpleFinAccount.id,
                      linkedAccountGuid: value,
                    });
                  }}
                  nothingFoundMessage={t("no_valid_accounts_found")}
                  elevation={2}
                />
              </Group>
            ) : (
              <Group gap="0.25rem">
                <Trans
                  i18nKey="linked_account_styled"
                  values={{
                    accountName: props.simpleFinAccount.linkedAccountId
                      ? getAccountNameForId(
                          props.simpleFinAccount.linkedAccountId,
                        )
                      : t("none"),
                  }}
                  components={[
                    <DimmedText size="xs" key="label" />,
                    getBadgeForAccountName(),
                  ]}
                />
                {isLinkedAccountDeleted && (
                  <Badge size="sm" color="var(--button-color-destructive)">
                    {t("deleted")}
                  </Badge>
                )}
              </Group>
            )}
            {isEditable ? (
              <Group gap="0.5rem">
                <PrimaryText size="xs">
                  {t("sync_start_date_input")}
                </PrimaryText>
                <DateInput
                  size="xs"
                  w="8rem"
                  {...syncStartDateField.getInputProps()}
                  onChange={(value) => {
                    syncStartDateField.setValue(value);
                    doUpdateSyncStartDate.mutate({
                      simpleFinAccountGuid: props.simpleFinAccount.id,
                      syncStartDate: dayjs(value).isValid()
                        ? dayjs(value).toDate()
                        : null,
                    });
                  }}
                  clearable
                  placeholder={t("auto")}
                  valueFormat={dateFormat}
                  locale={dayjsLocale}
                  elevation={2}
                />
              </Group>
            ) : (
              <Group gap="0.25rem">
                <Trans
                  i18nKey="sync_start_date_styled"
                  values={{
                    startDate: dayjs(
                      props.simpleFinAccount.syncStartDate,
                    ).isValid()
                      ? dayjs(props.simpleFinAccount.syncStartDate).format(
                          `${dateFormat}`,
                        )
                      : t("auto"),
                  }}
                  components={[
                    <DimmedText size="xs" key="label" />,
                    getBadgeForSyncStartDate(),
                  ]}
                />
              </Group>
            )}
            <DimmedText size="xs">
              {t("last_sync", {
                date: dayjs(props.simpleFinAccount.lastSync).isValid()
                  ? dayjs(props.simpleFinAccount.lastSync).format(
                      `${dateFormat} LT`,
                    )
                  : t("never"),
              })}
            </DimmedText>
          </Group>
          <DimmedText size="xs">
            {t("last_updated", {
              date: dayjs(props.simpleFinAccount.balanceDate).isValid()
                ? dayjs(props.simpleFinAccount.balanceDate).format(
                    `${dateFormat} LT`,
                  )
                : t("never"),
            })}
          </DimmedText>
        </Group>
      </Stack>
    </Card>
  );
};

export default SimpleFinAccountCard;
