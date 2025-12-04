import { ActionIcon, Flex, Group, LoadingOverlay } from "@mantine/core";
import { ICategoryResponse } from "~/models/category";
import { TrashIcon } from "lucide-react";
import React from "react";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import { notifications } from "@mantine/notifications";
import { translateAxiosError } from "~/helpers/requests";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import Card from "~/components/core/Card/Card";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";

interface CustomCategoryCardProps {
  category: ICategoryResponse;
}

const CustomCategoryCard = (
  props: CustomCategoryCardProps
): React.ReactNode => {
  const { request } = useAuth();

  const queryClient = useQueryClient();
  const doDeleteCategory = useMutation({
    mutationFn: async (guid: string) =>
      await request({
        url: "/api/transactionCategory",
        method: "DELETE",
        params: { guid },
      }),
    onSuccess: async () => {
      queryClient.invalidateQueries({ queryKey: ["transactionCategories"] });
      notifications.show({
        color: "var(--button-color-confirm)",
        message: "Category deleted!",
      });
    },
    onError: (error: AxiosError) =>
      notifications.show({ color: "red", message: translateAxiosError(error) }),
  });

  return (
    <Card elevation={2}>
      <LoadingOverlay visible={doDeleteCategory.isPending} />
      <Group>
        <Flex w="40%">
          <PrimaryText size="sm">{props.category.value}</PrimaryText>
        </Flex>
        <Flex w="40%">
          <PrimaryText size="sm">{props.category.parent}</PrimaryText>
        </Flex>
        <Flex justify="flex-end" flex="1 1 auto">
          <ActionIcon
            onClick={() => doDeleteCategory.mutate(props.category.id)}
            bg="var(--button-color-destructive)"
          >
            <TrashIcon size="1.2rem" />
          </ActionIcon>
        </Flex>
      </Group>
    </Card>
  );
};

export default CustomCategoryCard;
