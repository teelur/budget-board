import { Group, Skeleton, Stack } from "@mantine/core";
import React from "react";
import { IAutomaticRuleResponse } from "~/models/automaticRule";
import AddAutomaticRule from "./AddAutomaticRule/AddAutomaticRule";
import AutomaticRuleCard from "./AutomaticRuleCard/AutomaticRuleCard";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import { useTranslation } from "react-i18next";
import { useAutomaticRulesQuery } from "~/hooks/queries/useAutomaticRulesQuery";

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
          <DimmedText size="sm">{t("no_automatic_rules")}</DimmedText>
        </Group>
      );
    }

    return automaticRuleQuery.data?.map((rule: IAutomaticRuleResponse) => (
      <AutomaticRuleCard key={rule.id} rule={rule} />
    ));
  };

  return (
    <Stack gap="0.5rem">
      <DimmedText size="sm">{t("automatic_rules_description")}</DimmedText>
      <AddAutomaticRule />
      {getAutomaticRulesContent()}
    </Stack>
  );
};

export default AutomaticRules;
