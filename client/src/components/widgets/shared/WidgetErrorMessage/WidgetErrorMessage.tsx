import { Group } from "@mantine/core";
import { TriangleAlertIcon } from "lucide-react";
import { useTranslation } from "react-i18next";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";

interface WidgetErrorMessageProps {
  messageKey: string;
}

const WidgetErrorMessage = ({ messageKey }: WidgetErrorMessageProps) => {
  const { t } = useTranslation();

  return (
    <Group justify="center" align="center" gap="0.5rem" h="100%">
      <TriangleAlertIcon size={24} color="var(--base-color-text-dimmed)" />
      <DimmedText size="sm">{t(messageKey)}</DimmedText>
    </Group>
  );
};

export default WidgetErrorMessage;
