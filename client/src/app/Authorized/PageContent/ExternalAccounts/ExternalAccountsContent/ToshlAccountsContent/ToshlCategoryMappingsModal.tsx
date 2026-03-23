import {
  Button,
  Divider,
  LoadingOverlay,
  ScrollArea,
  SimpleGrid,
  Stack,
} from "@mantine/core";
import { useDisclosure } from "@mantine/hooks";
import { notifications } from "@mantine/notifications";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { AxiosError, AxiosResponse } from "axios";
import React from "react";
import { useTranslation } from "react-i18next";
import Modal from "~/components/core/Modal/Modal";
import CategorySelect from "~/components/core/Select/CategorySelect/CategorySelect";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import { translateAxiosError } from "~/helpers/requests";
import { ICategory } from "~/models/category";
import {
  IToshlCategoryMappingItem,
  IToshlCategoryMappingsResponse,
  IToshlCategoryMappingsUpdateRequest,
} from "~/models/toshl";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { useTransactionCategories } from "~/providers/TransactionCategoryProvider/TransactionCategoryProvider";

interface ToshlCategoryMappingsModalProps {
  disabled: boolean;
}

const ToshlCategoryMappingsModal = ({
  disabled,
}: ToshlCategoryMappingsModalProps): React.ReactNode => {
  const [opened, { open, close }] = useDisclosure(false);
  const [mappingValues, setMappingValues] = React.useState<Record<string, string>>(
    {}
  );
  const { transactionCategories } = useTransactionCategories();
  const { request } = useAuth();
  const { t } = useTranslation();
  const queryClient = useQueryClient();

  const mappingQuery = useQuery({
    queryKey: ["toshlCategoryMappings"],
    enabled: opened,
    queryFn: async (): Promise<IToshlCategoryMappingsResponse> => {
      const res: AxiosResponse = await request({
        url: "/api/toshl/categoryMappings",
        method: "GET",
      });

      return res.data as IToshlCategoryMappingsResponse;
    },
  });

  React.useEffect(() => {
    if (!mappingQuery.data?.items) {
      return;
    }

    setMappingValues(
      mappingQuery.data.items.reduce<Record<string, string>>((acc, item) => {
        acc[buildKey(item)] = item.budgetBoardCategory ?? "";
        return acc;
      }, {})
    );
  }, [mappingQuery.data]);

  const saveMappings = useMutation({
    mutationFn: async () =>
      await request({
        url: "/api/toshl/categoryMappings",
        method: "PUT",
        data: {
          items:
            mappingQuery.data?.items.map((item) => ({
              toshlName: item.toshlName,
              toshlId: item.toshlId,
              toshlType: item.toshlType,
              budgetBoardCategory: mappingValues[buildKey(item)] ?? "",
            })) ?? [],
        } as IToshlCategoryMappingsUpdateRequest,
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["toshlCategoryMappings"] });
      await queryClient.invalidateQueries({ queryKey: ["transactions"] });
      await queryClient.invalidateQueries({ queryKey: ["transactionCategories"] });
      notifications.show({
        message: t("toshl_category_mappings_saved", {
          defaultValue: "Toshl mappings saved.",
        }),
      });
      close();
    },
    onError: (error: AxiosError) => {
      notifications.show({
        color: "var(--button-color-destructive)",
        message: translateAxiosError(error),
      });
    },
  });

  const categories = React.useMemo<ICategory[]>(() => transactionCategories, [transactionCategories]);

  const groupedItems = React.useMemo(() => {
    const items = mappingQuery.data?.items ?? [];
    return {
      categories: items.filter((item) => item.toshlType === "category"),
      tags: items.filter((item) => item.toshlType === "tag"),
    };
  }, [mappingQuery.data]);

  return (
    <>
      <Button size="xs" variant="default" onClick={open} disabled={disabled}>
        {t("map_toshl_categories", { defaultValue: "Map Categories" })}
      </Button>
      <Modal
        opened={opened}
        onClose={close}
        size="lg"
        title={
          <PrimaryText>
            {t("toshl_category_mappings", {
              defaultValue: "Toshl Category Mappings",
            })}
          </PrimaryText>
        }
      >
        <LoadingOverlay visible={mappingQuery.isLoading || saveMappings.isPending} />
        <Stack gap="0.5rem">
          <DimmedText size="xs">
            {t("toshl_category_mappings_description", {
              defaultValue:
                "Map Toshl categories and tags to Budget Board categories. Saving reapplies the mapping to existing Toshl-imported transactions.",
            })}
          </DimmedText>
          <ScrollArea.Autosize mah="70vh">
            <SimpleGrid cols={{ base: 1, md: 2 }} spacing="md" pr="0.25rem">
              <Stack gap="0.5rem">
                <PrimaryText size="sm">
                  {t("toshl_categories", { defaultValue: "Toshl Categories" })}
                </PrimaryText>
                {groupedItems.categories.map((item) => (
                  <MappingRow
                    key={buildKey(item)}
                    item={item}
                    value={mappingValues[buildKey(item)] ?? ""}
                    categories={categories}
                    onChange={(value) =>
                      setMappingValues((current) => ({
                        ...current,
                        [buildKey(item)]: value,
                      }))
                    }
                  />
                ))}
              </Stack>
              <Stack gap="0.5rem">
                <PrimaryText size="sm">
                  {t("toshl_tags", { defaultValue: "Toshl Tags" })}
                </PrimaryText>
                {groupedItems.tags.map((item) => (
                  <MappingRow
                    key={buildKey(item)}
                    item={item}
                    value={mappingValues[buildKey(item)] ?? ""}
                    categories={categories}
                    onChange={(value) =>
                      setMappingValues((current) => ({
                        ...current,
                        [buildKey(item)]: value,
                      }))
                    }
                  />
                ))}
              </Stack>
            </SimpleGrid>
          </ScrollArea.Autosize>
          <Button onClick={() => saveMappings.mutate()} loading={saveMappings.isPending}>
            {t("save_toshl_mappings", {
              defaultValue: "Save Toshl Mappings",
            })}
          </Button>
        </Stack>
      </Modal>
    </>
  );
};

interface MappingRowProps {
  item: IToshlCategoryMappingItem;
  value: string;
  categories: ICategory[];
  onChange: (value: string) => void;
}

const MappingRow = ({
  item,
  value,
  categories,
  onChange,
}: MappingRowProps): React.ReactNode => {
  const title =
    item.toshlType === "tag" && item.toshlParentName
      ? `${item.toshlParentName} / ${item.toshlName}`
      : item.toshlName;

  return (
    <Stack gap="0.2rem">
      <PrimaryText size="sm">{title}</PrimaryText>
      {item.toshlType !== "tag" && item.toshlParentName ? (
        <DimmedText size="xs">{item.toshlParentName}</DimmedText>
      ) : null}
      <CategorySelect
        categories={categories}
        value={value}
        onChange={onChange}
        elevation={0}
        withinPortal
      />
      <Divider />
    </Stack>
  );
};

const buildKey = (
  item: Pick<IToshlCategoryMappingItem, "toshlId" | "toshlName" | "toshlType">
) => `${item.toshlType}::${item.toshlId || item.toshlName}`;

export default ToshlCategoryMappingsModal;
