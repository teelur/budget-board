import { Button, Group, SegmentedControl, Stack } from "@mantine/core";
import { useDisclosure } from "@mantine/hooks";
import { Repeat2Icon } from "lucide-react";
import React from "react";
import { useTranslation } from "react-i18next";
import ModalContentHeading from "~/components/core/Heading/PrimaryHeading/PrimaryHeading";
import Modal from "~/components/core/Modal/Modal";
import RecurringRuleForm from "~/components/RecurringRuleForm/RecurringRuleForm";
import Select from "~/components/core/Select/Select/Select";
import { useAssignRecurringTransactionsMutation } from "~/hooks/mutations/recurringRules/useAssignRecurringTransactionsMutation";
import { useRecurringRulesQuery } from "~/hooks/queries/useRecurringRulesQuery";
import { ITransaction } from "~/models/transaction";

interface BulkRecurringRuleActionProps {
  transactions: ITransaction[];
  onSuccess: () => void;
}

type RecurringRuleMode = "new" | "existing";

const BulkRecurringRuleAction = (
  props: BulkRecurringRuleActionProps,
): React.ReactNode => {
  const { t } = useTranslation();
  const [opened, { open, close }] = useDisclosure(false);
  const [selectedRuleID, setSelectedRuleID] = React.useState<string | null>(
    null,
  );
  const [mode, setMode] = React.useState<RecurringRuleMode>("new");
  const rulesQuery = useRecurringRulesQuery();
  const assignMutation = useAssignRecurringTransactionsMutation();
  const firstTransaction = [...props.transactions].sort((first, second) =>
    first.date.localeCompare(second.date),
  )[0];

  const selectedAccountID = props.transactions[0]?.accountID ?? null;
  const hasSingleAccount =
    selectedAccountID !== null &&
    props.transactions.every(
      (transaction) => transaction.accountID === selectedAccountID,
    );
  const hasExistingAssignment = props.transactions.some(
    (transaction) => transaction.recurringRuleID !== null,
  );

  const closeModal = () => {
    setMode("new");
    setSelectedRuleID(null);
    close();
  };

  const hasExistingRules = (rulesQuery.data ?? []).length > 0;

  const assignExistingRule = () => {
    if (!selectedRuleID) {
      return;
    }

    assignMutation.mutate(
      {
        recurringRuleID: selectedRuleID,
        transactionIDs: props.transactions.map((transaction) => transaction.id),
      },
      {
        onSuccess: () => {
          closeModal();
          props.onSuccess();
        },
      },
    );
  };

  if (props.transactions.length < 2 || hasExistingAssignment) {
    return null;
  }

  return (
    <>
      <Button
        size="compact-sm"
        variant="subtle"
        leftSection={<Repeat2Icon size="0.85rem" />}
        onClick={() => {
          setMode("new");
          setSelectedRuleID(null);
          open();
        }}
      >
        {t("assign_recurring_rule")}
      </Button>
      <Modal
        opened={opened}
        onClose={closeModal}
        title={
          <ModalContentHeading order={4}>
            {mode === "new"
              ? t("mark_as_recurring")
              : t("use_existing_recurring_rule")}
          </ModalContentHeading>
        }
      >
        <Stack gap="0.75rem">
          {hasExistingRules && (
            <SegmentedControl
              fullWidth
              value={mode}
              onChange={(value) => setMode(value as RecurringRuleMode)}
              data={[
                { value: "new", label: t("add_recurring_rule") },
                {
                  value: "existing",
                  label: t("use_existing_recurring_rule"),
                },
              ]}
            />
          )}
          {mode === "new" ? (
            <RecurringRuleForm
              transaction={firstTransaction}
              transactionIDs={props.transactions.map(
                (transaction) => transaction.id,
              )}
              onSuccess={() => {
                closeModal();
                props.onSuccess();
              }}
              onCancel={closeModal}
            />
          ) : (
            <>
              <Select
                label={t("use_existing_recurring_rule")}
                placeholder={t("select_existing_recurring_rule")}
                data={rulesQuery.data?.map((rule) => ({
                  value: rule.id,
                  label: `${rule.merchantName || t("any_merchant")} - ${rule.accountName}`,
                  disabled:
                    !hasSingleAccount || rule.accountID !== selectedAccountID,
                }))}
                value={selectedRuleID}
                onChange={setSelectedRuleID}
                searchable
                clearable
                elevation={0}
              />
              <Group>
                <Button
                  variant="outline"
                  disabled={!selectedRuleID || !hasSingleAccount}
                  loading={assignMutation.isPending}
                  onClick={assignExistingRule}
                >
                  {t("assign_recurring_rule")}
                </Button>
              </Group>
            </>
          )}
        </Stack>
      </Modal>
    </>
  );
};

export default BulkRecurringRuleAction;
