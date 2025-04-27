import classes from "./UnbudgetedCard.module.css";

import { convertNumberToCurrency } from "~/helpers/currency";
import {
  ActionIcon,
  Box,
  Card,
  Group,
  LoadingOverlay,
  Stack,
  Text,
} from "@mantine/core";
import { notifications } from "@mantine/notifications";
import { IBudgetCreateRequest } from "~/models/budget";
import { useQueryClient, useMutation } from "@tanstack/react-query";
import { AxiosError } from "axios";
import { PlusIcon } from "lucide-react";
import React from "react";
import { AuthContext } from "~/components/AuthProvider/AuthProvider";
import { translateAxiosError } from "~/helpers/requests";
import { ICategoryNode } from "~/models/category";
import UnbudgetedChildCard from "./UnbudgetedChildCard/UnbudgetedChildCard";
import { roundAwayFromZero } from "~/helpers/utils";

interface UnbudgetedCardProps {
  categoryTree: ICategoryNode;
  categoryToTransactionsTotalMap: Map<string, number>;
  selectedDate?: Date;
}

const UnbudgetedCard = (props: UnbudgetedCardProps): React.ReactNode => {
  const { request } = React.useContext<any>(AuthContext);

  const queryClient = useQueryClient();
  const doAddBudget = useMutation({
    mutationFn: async (newBudget: IBudgetCreateRequest[]) =>
      await request({
        url: "/api/budget",
        method: "POST",
        data: newBudget,
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["budgets"] });
    },
    onError: (error: AxiosError) => {
      notifications.show({ message: translateAxiosError(error), color: "red" });
    },
  });

  if (
    roundAwayFromZero(
      props.categoryToTransactionsTotalMap.get(
        props.categoryTree.value.toLocaleLowerCase()
      ) ?? 0
    ) === 0
  ) {
    return null;
  }

  return (
    <Stack gap="0.5rem" w="100%">
      <Card className={classes.root} radius="md" p="0.5rem">
        <LoadingOverlay visible={doAddBudget.isPending} />
        <Group w="100%" justify="space-between">
          <Text size="1rem" fw={600}>
            {props.categoryTree.value.length === 0
              ? "Uncategorized"
              : props.categoryTree.value}
          </Text>
          <Group gap="sm">
            <Text size="1rem" fw={600}>
              {convertNumberToCurrency(
                props.categoryToTransactionsTotalMap.get(
                  props.categoryTree.value.toLocaleLowerCase()
                ) ?? 0,
                false
              )}
            </Text>
            {props.selectedDate &&
            props.categoryTree.value !== "Uncategorized" ? (
              <ActionIcon
                size="sm"
                onClick={() =>
                  doAddBudget.mutate([
                    {
                      date: props.selectedDate!,
                      category: props.categoryTree.value,
                      limit: Math.round(
                        Math.abs(
                          props.categoryToTransactionsTotalMap.get(
                            props.categoryTree.value.toLocaleLowerCase()
                          ) ?? 0
                        )
                      ),
                    },
                  ])
                }
              >
                <PlusIcon />
              </ActionIcon>
            ) : (
              <Box h={22} w={22} />
            )}
          </Group>
        </Group>
      </Card>
      {props.categoryTree.subCategories.length > 0 && (
        <Stack gap="0.5rem">
          {props.categoryTree.subCategories.map((subCategory) => {
            if (
              !props.categoryToTransactionsTotalMap.has(
                subCategory.value.toLocaleLowerCase()
              )
            ) {
              return null;
            }
            return (
              <UnbudgetedChildCard
                key={subCategory.value}
                category={subCategory.value}
                amount={
                  props.categoryToTransactionsTotalMap.get(
                    subCategory.value.toLocaleLowerCase()
                  )!
                }
                selectedDate={props.selectedDate}
              />
            );
          })}
        </Stack>
      )}
    </Stack>
  );
};

export default UnbudgetedCard;
