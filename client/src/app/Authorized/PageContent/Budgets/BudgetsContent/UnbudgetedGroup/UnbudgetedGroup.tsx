import { Accordion, Group, Stack } from "@mantine/core";
import React from "react";
import UnbudgetedCard from "./UnbudgetedCard/UnbudgetedCard";
import { CategoryNode, ICategory, ICategoryNode } from "~/models/category";
import { convertNumberToCurrency } from "~/helpers/currency";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { useQuery } from "@tanstack/react-query";
import { IUserSettings } from "~/models/userSettings";
import { AxiosResponse } from "axios";
import PrimaryText from "~/components/Text/PrimaryText/PrimaryText";
import SurfaceAccordionRoot from "~/components/Accordion/Surface/SurfaceAccordionRoot/SurfaceAccordionRoot";
import DimmedText from "~/components/Text/DimmedText/DimmedText";

interface UnbudgetedGroupProps {
  categoryTree: ICategoryNode[];
  categoryToTransactionsTotalMap: Map<string, number>;
  categories: ICategory[];
  selectedDate: Date | null;
  openDetails: (category: string, month: Date | null) => void;
}

const UnbudgetedGroup = (props: UnbudgetedGroupProps): React.ReactNode => {
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

  const total =
    props.categoryTree.reduce((acc, category) => {
      const categoryTotal = props.categoryToTransactionsTotalMap.get(
        category.value.toLocaleLowerCase()
      );
      return acc + (categoryTotal ? categoryTotal : 0);
    }, 0) + (props.categoryToTransactionsTotalMap.get("") ?? 0);

  return (
    <SurfaceAccordionRoot defaultValue={[]}>
      <Accordion.Item value="unbudgeted">
        <Accordion.Control>
          <Group justify="space-between" align="center" w="100%" pr="1rem">
            <PrimaryText size="lg">Unbudgeted</PrimaryText>
            {userSettingsQuery.isPending ? null : (
              <PrimaryText size="lg">
                {convertNumberToCurrency(
                  total,
                  false,
                  userSettingsQuery.data?.currency ?? "USD"
                )}
              </PrimaryText>
            )}
          </Group>
        </Accordion.Control>
        <Accordion.Panel>
          <Stack gap="0.5rem">
            <UnbudgetedCard
              categoryTree={
                new CategoryNode({
                  value: "",
                  parent: "",
                })
              }
              categoryToTransactionsTotalMap={
                props.categoryToTransactionsTotalMap
              }
              selectedDate={props.selectedDate}
              openDetails={props.openDetails}
            />
            {props.categoryTree.length > 0 ? (
              props.categoryTree.map((categoryTree) => (
                <UnbudgetedCard
                  key={categoryTree.value}
                  categoryTree={categoryTree}
                  categoryToTransactionsTotalMap={
                    props.categoryToTransactionsTotalMap
                  }
                  selectedDate={props.selectedDate}
                  openDetails={props.openDetails}
                />
              ))
            ) : (
              <DimmedText size="sm">
                No unbudgeted transactions found.
              </DimmedText>
            )}
          </Stack>
        </Accordion.Panel>
      </Accordion.Item>
    </SurfaceAccordionRoot>
  );
};

export default UnbudgetedGroup;
