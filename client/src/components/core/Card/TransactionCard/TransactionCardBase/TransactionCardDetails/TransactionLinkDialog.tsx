import {
  Alert,
  Button,
  Group,
  Loader,
  Paper,
  Stack,
  Text,
} from "@mantine/core";
import { useDisclosure } from "@mantine/hooks";
import { AlertCircle, ArrowRightLeft, Link2, Unlink } from "lucide-react";
import React from "react";
import { useTranslation } from "react-i18next";
import { getCurrencySymbol } from "~/helpers/currency";
import { useLinkTransactionsMutation } from "~/hooks/mutations/transactions/useLinkTransactionsMutation";
import { useUnlinkTransactionMutation } from "~/hooks/mutations/transactions/useUnlinkTransactionMutation";
import { useTransactionLinkCandidatesQuery } from "~/hooks/queries/useTransactionLinkCandidatesQuery";
import { ITransaction } from "~/models/transaction";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import { useUserSettings } from "~/providers/UserSettingsProvider/UserSettingsProvider";
import Modal from "~/components/core/Modal/Modal";
import PrimaryHeading from "~/components/core/Heading/PrimaryHeading/PrimaryHeading";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";

interface TransactionLinkDialogProps {
  transaction: ITransaction;
}

const TransactionLinkDialog = ({
  transaction,
}: TransactionLinkDialogProps): React.ReactNode => {
  const { t } = useTranslation();
  const { dayjs, longDateFormat } = useLocale();
  const { preferredCurrency } = useUserSettings();
  const [linkOpened, { open: openLink, close: closeLink }] =
    useDisclosure(false);
  const [unlinkOpened, { open: openUnlink, close: closeUnlink }] =
    useDisclosure(false);
  const [selectedTransactionID, setSelectedTransactionID] = React.useState<
    string | null
  >(null);
  const candidatesQuery = useTransactionLinkCandidatesQuery(
    transaction.id,
    linkOpened,
  );
  const linkMutation = useLinkTransactionsMutation();
  const unlinkMutation = useUnlinkTransactionMutation();

  const formatAmount = (amount: number) =>
    `${getCurrencySymbol(preferredCurrency)}${amount.toFixed(2)}`;
  const formatDate = (date: string) => dayjs(date).format(longDateFormat);

  const onOpenLink = () => {
    setSelectedTransactionID(null);
    openLink();
  };

  const onLink = () => {
    if (!selectedTransactionID) {
      return;
    }

    linkMutation.mutate(
      {
        transactionID: transaction.id,
        linkedTransactionID: selectedTransactionID,
      },
      { onSuccess: closeLink },
    );
  };

  const onUnlink = () => {
    unlinkMutation.mutate(transaction.id, { onSuccess: closeUnlink });
  };

  const linkedDetails = transaction.linkedTransactionID ? (
    <Stack gap="0.25rem">
      <Button
        variant="subtle"
        size="compact-xs"
        leftSection={<Unlink size="0.85rem" />}
        onClick={(event) => {
          event.stopPropagation();
          openUnlink();
        }}
        loading={unlinkMutation.isPending}
      >
        {t("unlink_transactions")}
      </Button>
    </Stack>
  ) : (
    <Button
      variant="subtle"
      size="compact-xs"
      leftSection={<ArrowRightLeft size="0.85rem" />}
      onClick={(event) => {
        event.stopPropagation();
        onOpenLink();
      }}
    >
      {t("link_transfer")}
    </Button>
  );

  return (
    <>
      {linkedDetails}
      <Modal
        opened={linkOpened}
        onClose={closeLink}
        title={<PrimaryHeading order={4}>{t("link_transfer")}</PrimaryHeading>}
      >
        <Stack gap="sm">
          <DimmedText size="sm">
            {t("link_transfer_source", {
              account: transaction.accountName,
              amount: formatAmount(transaction.amount),
              date: formatDate(transaction.date),
            })}
          </DimmedText>
          <Alert
            color="orange"
            icon={<AlertCircle size="1rem" />}
            title={t("transfer_category_warning_title")}
          >
            {t("transfer_category_warning")}
          </Alert>
          {candidatesQuery.isPending ? (
            <Group justify="center">
              <Loader size="sm" />
            </Group>
          ) : candidatesQuery.isError ? (
            <Alert color="red">{t("transaction_link_candidates_error")}</Alert>
          ) : candidatesQuery.data?.length ? (
            <Stack gap="xs">
              {candidatesQuery.data.map((candidate) => {
                const isSelected = candidate.id === selectedTransactionID;
                return (
                  <Paper
                    key={candidate.id}
                    withBorder
                    p="xs"
                    bg={
                      isSelected ? "var(--mantine-color-blue-light)" : undefined
                    }
                  >
                    <Button
                      variant="subtle"
                      fullWidth
                      justify="space-between"
                      onClick={() => setSelectedTransactionID(candidate.id)}
                      aria-pressed={isSelected}
                    >
                      <Stack gap={0} align="flex-start">
                        <Text size="sm" fw={600}>
                          {candidate.accountName || t("unknown_account")}
                        </Text>
                        <Text size="xs">
                          {candidate.merchantName || t("no_merchant_name")} ·{" "}
                          {formatDate(candidate.date)}
                        </Text>
                      </Stack>
                      <Text size="sm">{formatAmount(candidate.amount)}</Text>
                    </Button>
                  </Paper>
                );
              })}
            </Stack>
          ) : (
            <DimmedText size="sm">{t("no_transfer_candidates")}</DimmedText>
          )}
          <Button
            leftSection={<Link2 size="1rem" />}
            onClick={onLink}
            disabled={!selectedTransactionID || candidatesQuery.isError}
            loading={linkMutation.isPending}
          >
            {t("confirm_link")}
          </Button>
        </Stack>
      </Modal>
      <Modal
        opened={unlinkOpened}
        onClose={closeUnlink}
        title={
          <PrimaryHeading order={4}>{t("unlink_transactions")}</PrimaryHeading>
        }
      >
        <Stack gap="sm">
          <DimmedText size="sm">{t("unlink_transfer_message")}</DimmedText>
          <Group justify="flex-end">
            <Button variant="default" onClick={closeUnlink}>
              {t("cancel")}
            </Button>
            <Button
              color="red"
              leftSection={<Unlink size="1rem" />}
              onClick={onUnlink}
              loading={unlinkMutation.isPending}
            >
              {t("unlink_transactions")}
            </Button>
          </Group>
        </Stack>
      </Modal>
    </>
  );
};

export default TransactionLinkDialog;
