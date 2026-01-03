import { ActionIcon, Badge, Group, LoadingOverlay, Stack } from "@mantine/core";
import { useField } from "@mantine/form";
import { useDisclosure } from "@mantine/hooks";
import { notifications } from "@mantine/notifications";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
import dayjs from "dayjs";
import { PencilIcon } from "lucide-react";
import React from "react";
import { Trans, useTranslation } from "react-i18next";
import Card from "~/components/core/Card/Card";
import AccountMultiSelect from "~/components/core/Select/AccountMultiSelect/AccountMultiSelect";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import StatusText from "~/components/core/Text/StatusText/StatusText";
import { convertNumberToCurrency } from "~/helpers/currency";
import { translateAxiosError } from "~/helpers/requests";
import { IAccountResponse } from "~/models/account";
import { ISimpleFinAccountResponse } from "~/models/simpleFinAccount";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

interface ISimpleFinAccountCardProps {
  simpleFinAccount: ISimpleFinAccountResponse;
}

const SimpleFinAccountCard = (
  props: ISimpleFinAccountCardProps
): React.ReactNode => {
  const [isEditable, { toggle }] = useDisclosure(false);

  const linkedAccountIdField = useField<string[]>({
    initialValue: props.simpleFinAccount.linkedAccountId
      ? [props.simpleFinAccount.linkedAccountId]
      : [],
  });

  const { t } = useTranslation();
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
        queryKey: ["simplefinOrganizations"],
      });
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
      (account) => account.id === accountId
    );
    return account ? account.name : t("unknown_account");
  };

  React.useEffect(() => {
    linkedAccountIdField.setValue(
      props.simpleFinAccount.linkedAccountId != null &&
        props.simpleFinAccount.linkedAccountId.length > 0
        ? [props.simpleFinAccount.linkedAccountId]
        : []
    );
  }, [props.simpleFinAccount.linkedAccountId]);

  const getBadgeForAccountName = (): React.ReactElement => {
    return props.simpleFinAccount.linkedAccountId ? (
      <Badge key="value" size="sm" />
    ) : (
      <Badge
        key="value"
        size="sm"
        color="var(--elevated-color-input-background)"
      />
    );
  };

  return (
    <Card elevation={2}>
      <LoadingOverlay visible={doUpdateLinkedAccount.isPending} />
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
              props.simpleFinAccount.currency
            )}
          </StatusText>
        </Group>
        <Group justify="space-between" align="center">
          <Group gap="0.5rem">
            {isEditable ? (
              <Group gap="0.5rem">
                <PrimaryText size="xs">{t("linked_account_input")}</PrimaryText>
                <AccountMultiSelect
                  size="xs"
                  value={
                    props.simpleFinAccount.linkedAccountId != null &&
                    props.simpleFinAccount.linkedAccountId.length > 0
                      ? [props.simpleFinAccount.linkedAccountId]
                      : []
                  }
                  maxSelectedValues={1}
                  onChange={(value) => {
                    const selectedAccountId =
                      value.length > 0 ? value[0] : null;
                    doUpdateLinkedAccount.mutate({
                      simpleFinAccountGuid: props.simpleFinAccount.id,
                      linkedAccountGuid: selectedAccountId ?? null,
                    });
                  }}
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
                          props.simpleFinAccount.linkedAccountId
                        )
                      : t("none"),
                  }}
                  components={[
                    <DimmedText size="xs" key="label" />,
                    getBadgeForAccountName(),
                  ]}
                />
              </Group>
            )}
            <DimmedText size="xs">
              {t("last_sync", {
                date: dayjs(props.simpleFinAccount.lastSync).isValid()
                  ? dayjs(props.simpleFinAccount.lastSync).format("L LT")
                  : t("never"),
              })}
            </DimmedText>
          </Group>
          <DimmedText size="xs">
            {t("last_updated", {
              date: dayjs(props.simpleFinAccount.balanceDate).isValid()
                ? dayjs(props.simpleFinAccount.balanceDate).format("L LT")
                : t("never"),
            })}
          </DimmedText>
        </Group>
      </Stack>
    </Card>
  );
};

export default SimpleFinAccountCard;
