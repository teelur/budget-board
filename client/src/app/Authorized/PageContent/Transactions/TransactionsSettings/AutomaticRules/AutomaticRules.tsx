import { Group, Skeleton, Stack } from "@mantine/core";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import React from "react";
import { useQuery } from "@tanstack/react-query";
import { IAutomaticRuleResponse } from "~/models/automaticRule";
import AddAutomaticRule from "./AddAutomaticRule/AddAutomaticRule";
import AutomaticRuleCard from "./AutomaticRuleCard/AutomaticRuleCard";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import { useTranslation } from "react-i18next";

const AutomaticRules = (): React.ReactNode => {
  const { t } = useTranslation();
  const { request } = useAuth();

  const automaticRuleQuery = useQuery({
    queryKey: ["automaticRule"],
    queryFn: async () => {
      const res = await request({
        url: "/api/automaticRule",
        method: "GET",
      });

      if (res.status === 200) {
        return res.data;
      }

      return undefined;
    },
  });

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
