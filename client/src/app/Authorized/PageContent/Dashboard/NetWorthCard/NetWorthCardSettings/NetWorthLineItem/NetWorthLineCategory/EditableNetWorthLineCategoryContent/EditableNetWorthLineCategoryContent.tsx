import { ActionIcon, Group } from "@mantine/core";
import { useField } from "@mantine/form";
import { useDidUpdate } from "@mantine/hooks";
import { ChevronRightIcon, PencilIcon } from "lucide-react";
import React from "react";
import CategorySelect from "~/components/core/Select/CategorySelect/CategorySelect";
import Select from "~/components/core/Select/Select/Select";
import { areStringsEqual } from "~/helpers/utils";
import { accountCategories } from "~/models/account";
import {
  INetWorthWidgetCategory,
  NET_WORTH_CATEGORY_ACCOUNT_SUBTYPES,
  NET_WORTH_CATEGORY_ASSET_SUBTYPES,
  NET_WORTH_CATEGORY_LINE_SUBTYPES,
  NET_WORTH_CATEGORY_TYPES,
} from "~/models/widgetSettings";
import { NetWorthSettingsContext } from "~/providers/NetWorthSettingsProvider/NetWorthSettingsProvider";

interface EditableNetWorthLineCategoryContentProps {
  category: INetWorthWidgetCategory;
  index: number;
  currentLineName: string;
  updateNetWorthCategory: (
    updatedCategory: INetWorthWidgetCategory,
    index: number
  ) => void;
  disableEdit: () => void;
}

const EditableNetWorthLineCategoryContent = (
  props: EditableNetWorthLineCategoryContentProps
): React.ReactNode => {
  const typeField = useField<string>({
    initialValue: props.category.type,
  });
  const subtypeField = useField<string>({
    initialValue: props.category.subtype,
  });
  const valueField = useField<string>({
    initialValue: props.category.value,
  });

  const networthsettings = React.useContext(NetWorthSettingsContext);

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

  const getValueOptions = (type: string, subtype: string) => {
    if (areStringsEqual(type, "account")) {
      if (areStringsEqual(subtype, "category")) {
        return accountCategories;
      }
    } else if (areStringsEqual(type, "asset")) {
      if (areStringsEqual(subtype, "all")) {
        return [];
      }
    } else if (areStringsEqual(type, "line")) {
      return networthsettings.lineNames.filter(
        (name) => name !== props.currentLineName
      );
    }
    return [];
  };

  useDidUpdate(() => {
    const type = typeField.getValue();
    const subtype = subtypeField.getValue();
    const value = valueField.getValue();

    const subtypeValues = getSubtypeOptions(type);
    if (!subtypeValues.find((st) => areStringsEqual(st, subtype))) {
      subtypeField.setValue("");
      return;
    }

    const valueValues = getValueOptions(type, subtype);
    if (
      valueValues.length > 0 &&
      !valueValues.find((v) => {
        const valueString = typeof v === "string" ? v : v.value;
        return areStringsEqual(valueString, value);
      })
    ) {
      valueField.setValue("");
      return;
    }

    if (type && subtype) {
      const updatedCategory: INetWorthWidgetCategory = {
        id: props.category.id,
        type,
        subtype,
        value,
      };
      props.updateNetWorthCategory(updatedCategory, props.index);
    }
  }, [typeField.getValue(), subtypeField.getValue(), valueField.getValue()]);

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
      // Lines can reference other lines by name; this will need to be populated elsewhere.
      return (
        <Select
          w="150px"
          size="xs"
          data={[...new Set(networthsettings.lineNames)].filter(
            (name) => name !== props.currentLineName
          )}
          {...valueField.getInputProps()}
          elevation={2}
        />
      );
    }

    return null;
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
          data={getSubtypeOptions(typeField.getValue())}
          {...subtypeField.getInputProps()}
          elevation={2}
        />
        <ActionIcon variant="outline" size="sm" onClick={props.disableEdit}>
          <PencilIcon size={14} />
        </ActionIcon>
      </Group>
      {getValidNetWorthValuesForTypeAndSubtype(
        typeField.getValue(),
        subtypeField.getValue()
      )}
    </Group>
  );
};

export default EditableNetWorthLineCategoryContent;
