import { Stack } from "@mantine/core";
import ForceSyncLookbackPeriod from "./ForceSyncLookbackPeriod/ForceSyncLookbackPeriod";
import DisableBuiltInTransactionCategories from "./DisableBuiltInTransactionCategories/DisableBuiltInTransactionCategories";
import EnableAutoCategorizer from "./EnableAutoCategorizer/EnableAutoCategorizer";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import { useTranslation } from "react-i18next";
import TrainAutoCategorizerModal from "./TrainAutoCategorizerModal/TrainAutoCategorizerModal";
import AutoCategorizerMinimumProbability from "./AutoCategorizerMinimumProbability/AutoCategorizerMinimumProbability";

const AdvancedSettings = () => {
  const { t } = useTranslation();

  return (
    <Stack gap="0.5rem">
      <PrimaryText size="lg">{t("advanced_settings")}</PrimaryText>
      <DisableBuiltInTransactionCategories />
      <ForceSyncLookbackPeriod />
      <EnableAutoCategorizer />
      <AutoCategorizerMinimumProbability />
      <TrainAutoCategorizerModal />
    </Stack>
  );
};

export default AdvancedSettings;
