import { convertNumberToCurrency, SignDisplay } from "~/helpers/currency";
import { ActionIcon, Group, LoadingOverlay, Stack } from "@mantine/core";
import { notifications } from "@mantine/notifications";
import { IBudgetCreateRequest } from "~/models/budget";
import { useQueryClient, useMutation, useQuery } from "@tanstack/react-query";
import { AxiosError, AxiosResponse } from "axios";
import { PlusIcon } from "lucide-react";
import React from "react";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { translateAxiosError } from "~/helpers/requests";
import { ICategoryNode } from "~/models/category";
import UnbudgetedChildCard from "./UnbudgetedChildCard/UnbudgetedChildCard";
import { roundAwayFromZero } from "~/helpers/utils";
import { IUserSettings } from "~/models/userSettings";
import { uncategorizedTransactionCategory } from "~/models/transaction";
import Card from "~/components/core/Card/Card";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import { useTranslation } from "react-i18next";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";

interface UnbudgetedCardProps {
  categoryTree: ICategoryNode;
  categoryToTransactionsTotalMap: Map<string, number>;
  selectedDate: Date | null;
  openDetails: (category: string, month: Date | null) => void;
}

const UnbudgetedCard = (props: UnbudgetedCardProps): React.ReactNode => {
  const { t } = useTranslation();
  const { intlLocale } = useLocale();
  const { request } = useAuth();

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
      notifications.show({
        message: translateAxiosError(error),
        color: "var(--button-color-destructive)",
      });
    },
  });

  if (
    roundAwayFromZero(
      props.categoryToTransactionsTotalMap.get(
        props.categoryTree.value.toLocaleLowerCase(),
      ) ?? 0,
    ) === 0
  ) {
    return null;
  }

  const getUnbudgetedChildCards = (): React.ReactNode => {
    if (props.categoryTree.subCategories.length === 0) {
      return null;
    }

    const childCards: React.ReactNode[] = [];

    props.categoryTree.subCategories.forEach((subCategory) => {
      if (
        !props.categoryToTransactionsTotalMap.has(
          subCategory.value.toLocaleLowerCase(),
        )
      ) {
        return;
      }
      childCards.push(
        <UnbudgetedChildCard
          key={subCategory.value}
          category={subCategory.value}
          amount={
            props.categoryToTransactionsTotalMap.get(
              subCategory.value.toLocaleLowerCase(),
            )!
          }
          selectedDate={props.selectedDate}
          openDetails={props.openDetails}
        />,
      );
    });

    if (childCards.length === 0) {
      return null;
    }

    return <Stack gap="0.5rem">{childCards}</Stack>;
  };

  return (
    <Stack gap="0.5rem" w="100%">
      <Card
        onClick={() => {
          if (props.selectedDate) {
            props.openDetails(props.categoryTree.value, props.selectedDate);
          }
        }}
        p="0.25rem"
        hoverEffect
        elevation={2}
      >
        <LoadingOverlay visible={doAddBudget.isPending} />
        <Group w="100%" justify="space-between">
          <PrimaryText size="md" fw={600}>
            {props.categoryTree.value.length === 0
              ? t(uncategorizedTransactionCategory)
              : props.categoryTree.value}
          </PrimaryText>
          <Group gap="sm">
            {userSettingsQuery.isPending ? null : (
              <PrimaryText size="1rem" fw={600}>
                {convertNumberToCurrency(
                  props.categoryToTransactionsTotalMap.get(
                    props.categoryTree.value.toLocaleLowerCase(),
                  ) ?? 0,
                  false,
                  userSettingsQuery.data?.currency ?? "USD",
                  SignDisplay.Auto,
                  intlLocale,
                )}
              </PrimaryText>
            )}
            {props.selectedDate && props.categoryTree.value.length !== 0 && (
              <ActionIcon
                size="sm"
                onClick={(event) => {
                  event.stopPropagation();
                  doAddBudget.mutate([
                    {
                      date: props.selectedDate!,
                      category: props.categoryTree.value,
                      limit: Math.round(
                        Math.abs(
                          props.categoryToTransactionsTotalMap.get(
                            props.categoryTree.value.toLocaleLowerCase(),
                          ) ?? 0,
                        ),
                      ),
                    },
                  ]);
                }}
              >
                <PlusIcon />
              </ActionIcon>
            )}
          </Group>
        </Group>
      </Card>
      {getUnbudgetedChildCards()}
    </Stack>
  );
};

export default UnbudgetedCard;
