import { ActionIcon, Button, Group, Stack } from "@mantine/core";
import { PencilIcon, PlayIcon, TrashIcon } from "lucide-react";
import React from "react";
import {
  IAutomaticRuleResponse,
  IRuleParameterEdit,
} from "~/models/automaticRule";
import ConditionItem from "./ConditionItem/ConditionItem";
import ActionItem from "./ActionItem/ActionItem";
import EditableAutomaticRuleContent from "../EditableAutomaticRuleContent/EditableAutomaticRuleContent";
import { useTransactionCategories } from "~/providers/TransactionCategoryProvider/TransactionCategoryProvider";
import Card from "~/components/core/Card/Card";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import { useTranslation } from "react-i18next";
import { useUserSettings } from "~/providers/UserSettingsProvider/UserSettingsProvider";
import { useAccountsQuery } from "~/hooks/queries/useAccountsQuery";
import { useDeleteAutomaticRuleMutation } from "~/hooks/mutations/automaticRules/useDeleteAutomaticRuleMutation";
import { useUpdateAutomaticRuleMutation } from "~/hooks/mutations/automaticRules/useUpdateAutomaticRuleMutation";
import { useRunAutomaticRuleMutation } from "~/hooks/mutations/automaticRules/useRunAutomaticRuleMutation";

interface AutomaticRuleCardProps {
  rule: IAutomaticRuleResponse;
}

const AutomaticRuleCard = (props: AutomaticRuleCardProps) => {
  const [isSelected, setIsSelected] = React.useState(false);

  const [conditionItems, setConditionItems] = React.useState<
    IRuleParameterEdit[]
  >(props.rule.conditions ?? []);
  const [actionItems, setActionItems] = React.useState<IRuleParameterEdit[]>(
    props.rule.actions ?? [],
  );

  const { t } = useTranslation();
  const { allTransactionCategories: transactionCategories } =
    useTransactionCategories();
  const { preferredCurrency } = useUserSettings();
  const accountsQuery = useAccountsQuery();
  const deleteAutomaticRuleMutation = useDeleteAutomaticRuleMutation();
  const updateAutomaticRuleMutation = useUpdateAutomaticRuleMutation();
  const runAutomaticRuleMutation = useRunAutomaticRuleMutation();

  if (isSelected) {
    return (
      <Card elevation={0}>
        <Stack gap="0.5rem">
          <EditableAutomaticRuleContent
            conditionItems={conditionItems}
            actionItems={actionItems}
            setConditionItems={setConditionItems}
            setActionItems={setActionItems}
          />
          <Group w="100%">
            <Button
              flex="1 1 auto"
              onClick={() => {
                updateAutomaticRuleMutation.mutate(
                  {
                    id: props.rule.id,
                    conditions: conditionItems.map(
                      (item) =>
                        ({
                          id: item.id ?? "",
                          value: item.value,
                          field: item.field,
                          operator: item.operator,
                        }) as IRuleParameterEdit,
                    ),
                    actions: actionItems.map(
                      (item) =>
                        ({
                          id: item.id ?? "",
                          value: item.value,
                          field: item.field,
                          operator: item.operator,
                        }) as IRuleParameterEdit,
                    ),
                  },
                  {
                    onSuccess: () => {
                      setIsSelected(false);
                    },
                  },
                );
              }}
              loading={updateAutomaticRuleMutation.isPending}
            >
              {t("save")}
            </Button>
            <Button
              flex="1 1 auto"
              variant="outline"
              onClick={() => setIsSelected(false)}
            >
              {t("cancel")}
            </Button>
          </Group>
        </Stack>
      </Card>
    );
  }

  return (
    <Card elevation={1}>
      <Group gap={0} justify="space-between" wrap="nowrap">
        <Stack>
          <Group gap="0.25rem">
            <DimmedText size="sm" pr="0.5rem">
              {t("if")}
            </DimmedText>
            {(props.rule.conditions ?? []).map((condition) => (
              <ConditionItem
                key={condition.id}
                condition={condition}
                categories={transactionCategories}
                currency={preferredCurrency}
                accounts={accountsQuery.data ?? []}
              />
            ))}
          </Group>
          <Group gap="0.25rem">
            <DimmedText size="sm" pr="0.5rem">
              {t("then")}
            </DimmedText>
            {(props.rule.actions ?? []).map((action) => (
              <ActionItem
                key={action.id}
                action={action}
                categories={transactionCategories}
                currency={preferredCurrency}
              />
            ))}
          </Group>
        </Stack>
        <Group style={{ alignSelf: "stretch" }} gap="0.5rem" wrap="nowrap">
          <ActionIcon
            variant="outline"
            onClick={() => {
              runAutomaticRuleMutation.mutate({
                conditions: conditionItems.map((item) => ({
                  field: item.field,
                  operator: item.operator,
                  value: item.value,
                })),
                actions: actionItems.map((item) => ({
                  field: item.field,
                  operator: item.operator,
                  value: item.value,
                })),
              });
            }}
            loading={runAutomaticRuleMutation.isPending}
            h="100%"
          >
            <PlayIcon size="1rem" />
          </ActionIcon>
          <ActionIcon
            onClick={() => {
              setConditionItems(props.rule.conditions ?? []);
              setActionItems(props.rule.actions ?? []);
              setIsSelected(true);
            }}
            h="100%"
          >
            <PencilIcon size="1rem" />
          </ActionIcon>
          <ActionIcon
            color="var(--button-color-destructive)"
            onClick={(e) => {
              e.stopPropagation();
              deleteAutomaticRuleMutation.mutate(props.rule.id);
            }}
            h="100%"
            loading={deleteAutomaticRuleMutation.isPending}
          >
            <TrashIcon size="1rem" />
          </ActionIcon>
        </Group>
      </Group>
    </Card>
  );
};

export default AutomaticRuleCard;
