import { Group, Skeleton, Stack } from "@mantine/core";
import React from "react";
import { IAutomaticRuleResponse } from "~/models/automaticRule";
import AddAutomaticRule from "./AddAutomaticRule/AddAutomaticRule";
import AutomaticRuleCard from "./AutomaticRuleCard/AutomaticRuleCard";
import { useTranslation } from "react-i18next";
import { useAutomaticRulesQuery } from "~/hooks/queries/useAutomaticRulesQuery";
import { BodyText } from "@teelur/budget-board-ui";

const AutomaticRules = (): React.ReactNode => {
  const { t } = useTranslation();
  const automaticRuleQuery = useAutomaticRulesQuery();

  const getAutomaticRulesContent = (): React.ReactNode => {
    if (automaticRuleQuery.isPending) {
      return <Skeleton height={46} radius="md" />;
    }

    if ((automaticRuleQuery.data ?? []).length === 0) {
      return (
        <Group justify="center" p="1rem">
          <BodyText tone="muted" size="sm">
            {t("no_automatic_rules")}
          </BodyText>
        </Group>
      );
    }

    return automaticRuleQuery.data?.map((rule: IAutomaticRuleResponse) => (
      <AutomaticRuleCard key={rule.id} rule={rule} />
    ));
  };

  return (
    <Stack gap="0.25rem">
      <Stack gap={0}>
        <BodyText tone="primary" size="md" fw={500}>
          {t("automatic_rules")}
        </BodyText>
        <BodyText tone="secondary" size="sm">
          {t("automatic_rules_description")}
        </BodyText>
      </Stack>
      <AddAutomaticRule />
      {getAutomaticRulesContent()}
    </Stack>
  );
};

export default AutomaticRules;
