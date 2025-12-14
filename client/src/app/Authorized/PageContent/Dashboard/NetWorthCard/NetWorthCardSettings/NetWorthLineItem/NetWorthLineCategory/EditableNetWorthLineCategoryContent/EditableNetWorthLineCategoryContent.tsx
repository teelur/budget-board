import { ActionIcon, Flex, Group } from "@mantine/core";
import { useField } from "@mantine/form";
import { useDidUpdate } from "@mantine/hooks";
import { notifications } from "@mantine/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import { ChevronRightIcon, PencilIcon, TrashIcon } from "lucide-react";
import React from "react";
import CategorySelect from "~/components/core/Select/CategorySelect/CategorySelect";
import Select from "~/components/core/Select/Select/Select";
import { translateAxiosError } from "~/helpers/requests";
import { areStringsEqual } from "~/helpers/utils";
import { accountCategories } from "~/models/account";
import { INetWorthWidgetCategoryUpdateRequest } from "~/models/netWorthWidgetConfiguration";
import {
  INetWorthWidgetCategory,
  INetWorthWidgetLine,
  NET_WORTH_CATEGORY_ACCOUNT_SUBTYPES,
  NET_WORTH_CATEGORY_ASSET_SUBTYPES,
  NET_WORTH_CATEGORY_LINE_SUBTYPES,
  NET_WORTH_CATEGORY_TYPES,
} from "~/models/widgetSettings";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

interface EditableNetWorthLineCategoryContentProps {
  category: INetWorthWidgetCategory;
  lineId: string;
  currentLineName: string;
  lines: INetWorthWidgetLine[];
  settingsId: string;
  disableEdit: () => void;
}

const EditableNetWorthLineCategoryContent = (
  props: EditableNetWorthLineCategoryContentProps
): React.ReactNode => {
  const typeField = useField<string | null>({
    initialValue: props.category.type,
  });
  const subtypeField = useField<string | null>({
    initialValue: props.category.subtype,
  });
  const valueField = useField<string | null>({
    initialValue: props.category.value,
  });

  const { request } = useAuth();

  const queryClient = useQueryClient();
  const doUpdateCategory = useMutation({
    mutationFn: async (updatedCategory: INetWorthWidgetCategoryUpdateRequest) =>
      await request({
        url: `/api/netWorthWidgetCategory`,
        method: "PUT",
        data: updatedCategory,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["widgetSettings"] });

      notifications.show({
        color: "var(--button-color-confirm)",
        message: "Net worth settings updated successfully.",
      });
    },
    onError: (error: AxiosError) => {
      notifications.show({
        color: "var(--button-color-destructive)",
        message: translateAxiosError(error),
      });
    },
  });

  const doDeleteCategory = useMutation({
    mutationFn: async () =>
      await request({
        url: `/api/netWorthWidgetCategory`,
        method: "DELETE",
        params: {
          categoryId: props.category.id,
          lineId: props.lineId,
          widgetSettingsId: props.settingsId,
        },
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["widgetSettings"] });

      notifications.show({
        color: "var(--button-color-confirm)",
        message: "Net worth settings deleted successfully.",
      });
    },
    onError: (error: AxiosError) => {
      notifications.show({
        color: "var(--button-color-destructive)",
        message: translateAxiosError(error),
      });
    },
  });

  const type = typeField.getValue();
  const subtype = subtypeField.getValue();
  const value = valueField.getValue();

  const isValueValid = (type: string, subtype: string, value: string) => {
    if (
      areStringsEqual(type, "account") &&
      areStringsEqual(subtype, "category")
    ) {
      return !!value;
    } else if (areStringsEqual(type, "asset")) {
      if (areStringsEqual(subtype, "all")) {
        return true;
      } else if (areStringsEqual(subtype, "specific")) {
        return !!value;
      }
    } else if (areStringsEqual(type, "line")) {
      if (areStringsEqual(subtype, "name")) {
        return !!value && value !== props.currentLineName;
      }
    }

    return false;
  };

  useDidUpdate(() => {
    subtypeField.setValue(null);
  }, [type]);

  useDidUpdate(() => {
    valueField.setValue(null);
  }, [subtype]);

  useDidUpdate(() => {
    if (type && subtype && isValueValid(type, subtype, value ?? "")) {
      doUpdateCategory.mutate({
        id: props.category.id,
        type,
        subtype,
        value: value ?? "",
        lineId: props.lineId,
        widgetSettingsId: props.settingsId,
      } as INetWorthWidgetCategoryUpdateRequest);
    }
  }, [type, subtype, value]);

  const getValidLineNames = () => {
    const linesThatUseThisName = props.lines.filter((line) => {
      return line.categories.some((category) => {
        return (
          areStringsEqual(category.type, "line") &&
          areStringsEqual(category.subtype, "name") &&
          areStringsEqual(category.value, props.currentLineName)
        );
      });
    });

    const validLineNames = props.lines
      .map((line) => line.name)
      .filter(
        (name) =>
          !linesThatUseThisName.some((line) =>
            areStringsEqual(line.name, name)
          ) && !areStringsEqual(name, props.currentLineName)
      );
    return [...new Set(validLineNames)];
  };

  const getValidNetWorthValuesForTypeAndSubtype = (
    type: string,
    subtype: string
  ): React.ReactNode => {
    if (areStringsEqual(type, "account")) {
      if (areStringsEqual(subtype, "category")) {
        return (
          <CategorySelect
            size="xs"
            categories={accountCategories}
            {...valueField.getInputProps()}
            withinPortal
            elevation={2}
          />
        );
      }
    } else if (areStringsEqual(type, "asset")) {
      if (areStringsEqual(subtype, "all")) {
        return null;
      }
    } else if (areStringsEqual(type, "line")) {
      if (areStringsEqual(subtype, "name")) {
        return (
          <Select
            w="150px"
            size="xs"
            data={getValidLineNames()}
            {...valueField.getInputProps()}
            elevation={2}
          />
        );
      }
    }

    return null;
  };

  const getSubtypeOptions = (type: string) => {
    switch (type?.toLowerCase()) {
      case "account":
        return NET_WORTH_CATEGORY_ACCOUNT_SUBTYPES;
      case "asset":
        return NET_WORTH_CATEGORY_ASSET_SUBTYPES;
      case "line":
        return NET_WORTH_CATEGORY_LINE_SUBTYPES;
      default:
        return [];
    }
  };

  return (
    <Group justify="space-between">
      <Group gap="0.25rem">
        <Select
          w="100px"
          size="xs"
          data={NET_WORTH_CATEGORY_TYPES}
          {...typeField.getInputProps()}
          elevation={2}
        />
        <ChevronRightIcon size={14} />
        <Select
          w="100px"
          size="xs"
          data={getSubtypeOptions(typeField.getValue() ?? "")}
          {...subtypeField.getInputProps()}
          elevation={2}
        />
        <ActionIcon variant="outline" size="sm" onClick={props.disableEdit}>
          <PencilIcon size={14} />
        </ActionIcon>
      </Group>
      <Group gap="0.25rem">
        {getValidNetWorthValuesForTypeAndSubtype(
          typeField.getValue() ?? "",
          subtypeField.getValue() ?? ""
        )}
        <Flex style={{ alignSelf: "stretch" }}>
          <ActionIcon
            color="var(--button-color-destructive)"
            h="100%"
            size="md"
            loading={doDeleteCategory.isPending}
            onClick={async () => await doDeleteCategory.mutateAsync()}
          >
            <TrashIcon size={16} />
          </ActionIcon>
        </Flex>
      </Group>
    </Group>
  );
};

export default EditableNetWorthLineCategoryContent;
