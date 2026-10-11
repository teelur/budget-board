import { Group } from "@mantine/core";
import { BodyText } from "@teelur/budget-board-ui";
import { TriangleAlertIcon } from "lucide-react";
import { useTranslation } from "react-i18next";

interface WidgetErrorMessageProps {
  messageKey: string;
}

const WidgetErrorMessage = ({ messageKey }: WidgetErrorMessageProps) => {
  const { t } = useTranslation();

  return (
    <Group justify="center" align="center" gap="0.5rem" h="100%">
      <TriangleAlertIcon size={24} color="var(--bbui-color-text-secondary)" />
      <BodyText tone="secondary" size="sm">
        {t(messageKey)}
      </BodyText>
    </Group>
  );
};

export default WidgetErrorMessage;
