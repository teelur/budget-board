import { ActionIcon, Badge, Group, LoadingOverlay, Stack } from "@mantine/core";
import { useField } from "@mantine/form";
import { useDisclosure } from "@mantine/hooks";
import { notifications } from "@mantine/notifications";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
import { PencilIcon } from "lucide-react";
import React from "react";
import { Trans, useTranslation } from "react-i18next";
import Card from "~/components/core/Card/Card";
import Select from "~/components/core/Select/Select/Select";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import StatusText from "~/components/core/Text/StatusText/StatusText";
import { convertNumberToCurrency, SignDisplay } from "~/helpers/currency";
import {
  lunchFlowAccountQueryKey,
  translateAxiosError,
} from "~/helpers/requests";
import { AccountSource, IAccountResponse } from "~/models/account";
import { ILunchFlowAccountResponse } from "~/models/lunchFlowAccount";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";

interface ILunchFlowAccountCardProps {
  lunchFlowAccount: ILunchFlowAccountResponse;
}

const LunchFlowAccountCard = (
  props: ILunchFlowAccountCardProps,
): React.ReactNode => {
  const [isEditable, { toggle }] = useDisclosure(false);

  const linkedAccountIdField = useField<string[]>({
    initialValue: props.lunchFlowAccount.linkedAccountId
      ? [props.lunchFlowAccount.linkedAccountId]
      : [],
  });

  const { t } = useTranslation();
  const { dayjs, dateFormat, intlLocale } = useLocale();
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

  const lunchFlowAccountsQuery = useQuery({
    queryKey: [lunchFlowAccountQueryKey],
    queryFn: async (): Promise<ILunchFlowAccountResponse[]> => {
      const res: AxiosResponse = await request({
        url: "/api/lunchFlowAccount",
        method: "GET",
      });

      if (res.status === 200) {
        return res.data as ILunchFlowAccountResponse[];
      }

      return [];
    },
  });

  const queryClient = useQueryClient();
  const doUpdateLinkedAccount = useMutation({
    mutationFn: async (updateLinkedAccountRequest: {
      lunchFlowAccountGuid: string;
      linkedAccountGuid: string | null;
    }) =>
      await request({
        url: "/api/lunchFlowAccount/updateLinkedAccount",
        method: "PUT",
        params: {
          lunchFlowAccountGuid: updateLinkedAccountRequest.lunchFlowAccountGuid,
          linkedAccountGuid: updateLinkedAccountRequest.linkedAccountGuid,
        },
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [lunchFlowAccountQueryKey] });
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

  const getAccountNameForId = (accountId: string): string => {
    const account = accountsQuery.data?.find(
      (account) => account.id === accountId,
    );
    return account ? account.name : t("unknown_account");
  };

  const isLinkedAccountDeleted = React.useMemo(() => {
    if (!props.lunchFlowAccount.linkedAccountId) {
      return false;
    }
    const linkedAccount = accountsQuery.data?.find(
      (account) => account.id === props.lunchFlowAccount.linkedAccountId,
    );
    return linkedAccount?.deleted != null;
  }, [accountsQuery.data, props.lunchFlowAccount.linkedAccountId]);

  React.useEffect(() => {
    linkedAccountIdField.setValue(
      props.lunchFlowAccount.linkedAccountId != null &&
        props.lunchFlowAccount.linkedAccountId.length > 0
        ? [props.lunchFlowAccount.linkedAccountId]
        : [],
    );
  }, [props.lunchFlowAccount.linkedAccountId]);

  const getBadgeForAccountName = (): React.ReactElement => {
    return props.lunchFlowAccount.linkedAccountId ? (
      <Badge key="value" size="sm" />
    ) : (
      <Badge key="value" size="sm" color="gray" />
    );
  };

  const selectableAccounts = React.useMemo(() => {
    const linkedAccountIds = lunchFlowAccountsQuery.data?.map(
      (lfa) => lfa.linkedAccountId,
    );

    return accountsQuery.data
      ?.filter(
        (account) =>
          (!linkedAccountIds?.includes(account.id) &&
            account.deleted == null &&
            account.source !== AccountSource.SimpleFIN) ||
          account.id === props.lunchFlowAccount.linkedAccountId,
      )
      .map((account) => ({
        value: account.id,
        label: account.name,
      }));
  }, [
    accountsQuery.data,
    lunchFlowAccountsQuery.data,
    props.lunchFlowAccount.linkedAccountId,
  ]);

  const accountCurrency = React.useMemo(() => {
    const accountCurrency = props.lunchFlowAccount.currency;
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
  }, [accountsQuery.data, props.lunchFlowAccount.linkedAccountId]);

  return (
    <Card elevation={2}>
      <LoadingOverlay visible={doUpdateLinkedAccount.isPending} />
      <Stack gap={0}>
        <Group justify="space-between" align="center">
          <Group gap="0.5rem">
            <PrimaryText size="sm">{props.lunchFlowAccount.name}</PrimaryText>
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
          <StatusText size="sm" amount={props.lunchFlowAccount.balance}>
            {convertNumberToCurrency(
              props.lunchFlowAccount.balance,
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
                  value={props.lunchFlowAccount.linkedAccountId}
                  onChange={(value) => {
                    doUpdateLinkedAccount.mutate({
                      lunchFlowAccountGuid: props.lunchFlowAccount.id,
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
                    accountName: props.lunchFlowAccount.linkedAccountId
                      ? getAccountNameForId(
                          props.lunchFlowAccount.linkedAccountId,
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
            <DimmedText size="xs">
              {t("last_sync", {
                date: dayjs(props.lunchFlowAccount.lastSync).isValid()
                  ? dayjs(props.lunchFlowAccount.lastSync).format(
                      `${dateFormat} LT`,
                    )
                  : t("never"),
              })}
            </DimmedText>
          </Group>
          <DimmedText size="xs">
            {t("last_updated", {
              date: dayjs(props.lunchFlowAccount.balanceDate).isValid()
                ? dayjs(props.lunchFlowAccount.balanceDate).format(
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

export default LunchFlowAccountCard;
