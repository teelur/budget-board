import classes from "./UncategorizedTransaction.module.css";

import { Card, Flex, LoadingOverlay, Text } from "@mantine/core";
import { ITransaction, ITransactionUpdateRequest } from "~/models/transaction";
import React from "react";
import { AuthContext } from "~/providers/AuthProvider/AuthProvider";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { translateAxiosError } from "~/helpers/requests";
import { notifications } from "@mantine/notifications";
import { AxiosError, AxiosResponse } from "axios";
import { ICategory } from "~/models/category";
import { convertNumberToCurrency } from "~/helpers/currency";
import { useDisclosure } from "@mantine/hooks";
import { IUserSettings } from "~/models/userSettings";
import EditableCategoryCell from "~/components/EditableCategoryCell/EditableCategoryCell";

interface TransactionCardProps {
  transaction: ITransaction;
  categories: ICategory[];
}

const UncategorizedTransaction = (
  props: TransactionCardProps
): React.ReactNode => {
  const [opened, { toggle }] = useDisclosure(false);

  const { request } = React.useContext<any>(AuthContext);

  const userSettingsQuery = useQuery({
    queryKey: ["userSettings"],
    queryFn: async (): Promise<IUserSettings | undefined> => {
      const res: AxiosResponse = await request({
        url: "/api/userSettings",
        method: "GET",
      });

      if (res.status === 200) {
        return res.data as IUserSettings;
      }

      return undefined;
    },
  });

  const queryClient = useQueryClient();
  const doEditTransaction = useMutation({
    mutationFn: async (newTransaction: ITransactionUpdateRequest) =>
      await request({
        url: "/api/transaction",
        method: "PUT",
        data: newTransaction,
      }),
    onMutate: async (variables: ITransactionUpdateRequest) => {
      await queryClient.cancelQueries({ queryKey: ["transactions"] });

      const previousTransactions: ITransaction[] =
        queryClient.getQueryData(["transactions", { getHidden: false }]) ?? [];

      queryClient.setQueryData(
        ["transactions", { getHidden: false }],
        (oldTransactions: ITransaction[]) => {
          oldTransactions.map((oldTransaction) =>
            oldTransaction.id === variables.id
              ? {
                  ...oldTransaction,
                  category: variables.category,
                  subcategory: variables.subcategory,
                }
              : oldTransaction
          );
        }
      );

      return { previousTransactions };
    },
    onError: (
      error: AxiosError,
      _variables: ITransactionUpdateRequest,
      context
    ) => {
      queryClient.setQueryData(
        ["transactions", { getHidden: false }],
        context?.previousTransactions ?? []
      );
      notifications.show({ color: "red", message: translateAxiosError(error) });
    },
    onSettled: () => {
      queryClient.invalidateQueries({ queryKey: ["transactions"] });
      queryClient.invalidateQueries({ queryKey: ["balances"] });
      queryClient.invalidateQueries({ queryKey: ["accounts"] });
      queryClient.invalidateQueries({ queryKey: ["institutions"] });
    },
  });

  return (
    <Card
      className={classes.root}
      radius="md"
      onClick={toggle}
      bg={opened ? "var(--mantine-primary-color-light)" : ""}
    >
      <LoadingOverlay visible={doEditTransaction.isPending} />
      <Flex className={classes.container}>
        <Flex
          className={classes.leftSubContainer}
          direction={{ base: "column", xs: "row" }}
        >
          <Flex w={{ base: "100%", xs: "160px" }}>
            <Text>
              {new Date(props.transaction.date).toLocaleDateString([], {
                year: "numeric",
                month: "long",
                day: "numeric",
              })}
            </Text>
          </Flex>
          <Flex w="100%">
            <Text>{props.transaction.merchantName}</Text>
          </Flex>
        </Flex>
        <Flex
          className={classes.rightSubContainer}
          direction={{ base: "column", xs: "row" }}
        >
          <EditableCategoryCell
            transaction={props.transaction}
            categories={props.categories}
            isSelected={opened}
            editCell={doEditTransaction.mutate}
          />
          <Flex w={{ base: "100%", xs: "90px" }}>
            {userSettingsQuery.isPending ? null : (
              <Text
                style={{
                  color:
                    props.transaction.amount < 0
                      ? "var(--mantine-color-red-6)"
                      : "var(--mantine-color-green-6)",
                  fontWeight: 600,
                }}
              >
                {convertNumberToCurrency(
                  props.transaction.amount,
                  true,
                  userSettingsQuery.data?.currency ?? "USD"
                )}
              </Text>
            )}
          </Flex>
        </Flex>
      </Flex>
    </Card>
  );
};

export default UncategorizedTransaction;
