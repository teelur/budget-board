import { ActionIcon, Flex, Group } from "@mantine/core";
import { ChevronRightIcon, PencilIcon } from "lucide-react";
import { useTranslation } from "react-i18next";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import { getSubtypeOptions, NET_WORTH_CATEGORY_TYPES } from "~/helpers/widgets";
import { INetWorthWidgetCategory } from "~/models/widgetSettings";

interface NetWorthLineCategoryContentProps {
  category: INetWorthWidgetCategory;
  enableEdit: () => void;
}

const NetWorthLineCategoryContent = (
  props: NetWorthLineCategoryContentProps
) => {
  const { t } = useTranslation();

  const getTypeDisplayString = (): string => {
    const type = NET_WORTH_CATEGORY_TYPES.find(
      (type) => type.value === props.category.type
    );
    return type ? t(type.label) : props.category.type;
  };

  const getSubtypeDisplayString = (): string => {
    const subtypeOptions = getSubtypeOptions(props.category.type);
    const subtype = subtypeOptions.find(
      (subtype) => subtype.value === props.category.subtype
    );
    return subtype ? t(subtype.label) : props.category.subtype;
  };

  return (
    <Flex direction={{ base: "column", xs: "row" }} justify="space-between">
      <Group gap="0.25rem" align="center">
        {props.category.type.length > 0 ? (
          <PrimaryText size="sm">{getTypeDisplayString()}</PrimaryText>
        ) : (
          <DimmedText size="sm">{t("no_type")}</DimmedText>
        )}
        <ChevronRightIcon size={14} />
        {props.category.subtype.length > 0 ? (
          <DimmedText size="sm">{getSubtypeDisplayString()}</DimmedText>
        ) : (
          <DimmedText size="sm">{t("no_subtype")}</DimmedText>
        )}
        <ActionIcon variant="transparent" size="sm" onClick={props.enableEdit}>
          <PencilIcon size={14} />
        </ActionIcon>
      </Group>

      <PrimaryText size="sm">{props.category.value}</PrimaryText>
    </Flex>
  );
};

export default NetWorthLineCategoryContent;
