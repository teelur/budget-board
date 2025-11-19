import { Stack, Text } from "@mantine/core";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import React from "react";
import { useQuery } from "@tanstack/react-query";
import { IAutomaticRuleResponse } from "~/models/automaticRule";
import AddAutomaticRule from "./AddAutomaticRule/AddAutomaticRule";
import AutomaticRuleCard from "./AutomaticRuleCard/AutomaticRuleCard";

const AutomaticRules = (): React.ReactNode => {
  const { request } = useAuth();

  const AutomaticRuleQuery = useQuery({
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

  return (
    <Stack gap="0.5rem">
      <Text c="dimmed" size="sm" fw={600}>
        Create rules that automatically update fields during sync when the
        specified conditions are met.
      </Text>
      <AddAutomaticRule />
      {AutomaticRuleQuery.data?.map((rule: IAutomaticRuleResponse) => (
        <AutomaticRuleCard key={rule.id} rule={rule} />
      ))}
    </Stack>
  );
};

export default AutomaticRules;
