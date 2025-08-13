import { Button, Card, TextInput, Stack } from "@mantine/core";
import { useField } from "@mantine/form";
import { notifications } from "@mantine/notifications";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import React from "react";
import { AuthContext } from "~/components/AuthProvider/AuthProvider";
import CategorySelect from "~/components/CategorySelect";
import { translateAxiosError } from "~/helpers/requests";
import { IAutomaticCategorizationRuleRequest } from "~/models/automaticCategorizationRule";
import { ICategoryResponse } from "~/models/category";
import { defaultTransactionCategories } from "~/models/transaction";

const AddCategorizationRule = (): React.ReactNode => {
  const ruleField = useField<string>({
    initialValue: "",
    validate: (value) => {
      try {
        new RegExp(value);
        return null;
      } catch {
        return "Invalid rule name";
      }
    },
  });
  const categoryField = useField<string>({
    initialValue: "",
  });

  const { request } = React.useContext<any>(AuthContext);

  const transactionCategoriesQuery = useQuery({
    queryKey: ["transactionCategories"],
    queryFn: async () => {
      const res = await request({
        url: "/api/transactionCategory",
        method: "GET",
      });

      if (res.status === 200) {
        return res.data as ICategoryResponse[];
      }

      return undefined;
    },
  });

  const transactionCategoriesWithCustom = defaultTransactionCategories.concat(
    transactionCategoriesQuery.data ?? []
  );

  const queryClient = useQueryClient();
  const doAddRule = useMutation({
    mutationFn: async (
      automaticCategorizationRule: IAutomaticCategorizationRuleRequest
    ) =>
      await request({
        url: "/api/automaticCategorizationRule",
        method: "POST",
        data: automaticCategorizationRule,
      }),
    onSuccess: async () =>
      await queryClient.invalidateQueries({
        queryKey: ["automaticCategorizationRule"],
      }),
    onError: (error: AxiosError) => {
      notifications.show({ message: translateAxiosError(error), color: "red" });
    },
  });

  return (
    <Card withBorder>
      <Stack gap="0.5rem">
        <TextInput placeholder="Regex Rule" {...ruleField.getInputProps()} />
        <CategorySelect
          categories={transactionCategoriesWithCustom}
          {...categoryField.getInputProps()}
          withinPortal
        />
        <Button
          onClick={() => {
            doAddRule.mutate({
              categorizationRule: ruleField.getValue(),
              category: categoryField.getValue(),
            });
          }}
        >
          Add Rule
        </Button>
      </Stack>
    </Card>
  );
};

export default AddCategorizationRule;
