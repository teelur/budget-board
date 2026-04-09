import { Stack } from "@mantine/core";
import ForceSyncLookbackPeriod from "./ForceSyncLookbackPeriod/ForceSyncLookbackPeriod";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import { useTranslation } from "react-i18next";

const AdvancedSettings = () => {
  const { t } = useTranslation();

  return (
    <Stack gap="0.5rem">
      <PrimaryText size="lg">{t("advanced_settings")}</PrimaryText>
      <ForceSyncLookbackPeriod />
    </Stack>
  );
};

export default AdvancedSettings;
