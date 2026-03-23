import { Stack } from "@mantine/core";
import ForceSyncLookbackPeriod from "./ForceSyncLookbackPeriod/ForceSyncLookbackPeriod";
import DisableBuiltInTransactionCategories from "./DisableBuiltInTransactionCategories/DisableBuiltInTransactionCategories";
import EnableAutoCategorizer from "./EnableAutoCategorizer/EnableAutoCategorizer";
import Card from "~/components/core/Card/Card";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import { useTranslation } from "react-i18next";
import TrainAutoCategorizerModal from "./TrainAutoCategorizerModal/TrainAutoCategorizerModal";
import AutoCategorizerMinimumProbability from "./AutoCategorizerMinimumProbability/AutoCategorizerMinimumProbability";
import WipeAllDataButton from "./WipeAllDataButton/WipeAllDataButton";
import { Pages } from "../../PageContent";

interface AdvancedSettingsProps {
  setCurrentPage?: (page: Pages) => void;
}

const AdvancedSettings = ({ setCurrentPage }: AdvancedSettingsProps) => {
  const { t } = useTranslation();

  return (
    <Card elevation={1}>
      <Stack gap="0.5rem">
        <PrimaryText size="lg">{t("advanced_settings")}</PrimaryText>
        <DisableBuiltInTransactionCategories />
        <ForceSyncLookbackPeriod />
        <EnableAutoCategorizer />
        <AutoCategorizerMinimumProbability />
        <TrainAutoCategorizerModal />
        <WipeAllDataButton setCurrentPage={setCurrentPage} />
      </Stack>
    </Card>
  );
};

export default AdvancedSettings;
