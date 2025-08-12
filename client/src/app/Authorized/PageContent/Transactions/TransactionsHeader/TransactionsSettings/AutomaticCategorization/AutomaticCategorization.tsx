import { Stack, Text } from "@mantine/core";
import AddCategorizationRule from "./AddCategorizationRule/AddCategorizationRule";
import { AuthContext } from "~/components/AuthProvider/AuthProvider";
import React from "react";
import { useQuery } from "@tanstack/react-query";
import CategorizationRuleCard from "./CategorizationRuleCard/CategorizationRuleCard";
import { IAutomaticCategorizationRuleResponse } from "~/models/automaticCategorizationRule";

const AutomaticCategorization = (): React.ReactNode => {
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
        Create regex rules to automatically apply a category to your
        uncategorized transactions during sync.
      </Text>
      <AddCategorizationRule />
      {AutomaticCategorizationQuery.data?.map(
        (rule: IAutomaticCategorizationRuleResponse) => (
          <CategorizationRuleCard
            key={rule.id}
            id={rule.id}
            rule={rule.categorizationRule}
            category={rule.category}
          />
        )
      )}
    </Stack>
  );
};

export default AutomaticCategorization;
