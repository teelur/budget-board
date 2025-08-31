import { Stack, Text } from "@mantine/core";
import { AuthContext } from "~/components/AuthProvider/AuthProvider";
import React from "react";
import { useQuery } from "@tanstack/react-query";
import { IAutomaticCategorizationRuleResponse } from "~/models/automaticCategorizationRule";
import AddAutomaticRule from "./AddAutomaticRule/AddAutomaticRule";
import AutomaticRuleCard from "./AutomaticRuleCard/AutomaticRuleCard";

const AutomaticRules = (): React.ReactNode => {
  const { request } = React.useContext<any>(AuthContext);

  const AutomaticCategorizationQuery = useQuery({
    queryKey: ["automaticCategorizationRule"],
    queryFn: async () => {
      const res = await request({
        url: "/api/automaticCategorizationRule",
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
      {AutomaticCategorizationQuery.data?.map(
        (rule: IAutomaticCategorizationRuleResponse) => (
          <AutomaticRuleCard
            key={rule.id}
            id={rule.id}
            rule="rule.categorizationRule"
            category=""
          />
        )
      )}
    </Stack>
  );
};

export default AutomaticRules;
