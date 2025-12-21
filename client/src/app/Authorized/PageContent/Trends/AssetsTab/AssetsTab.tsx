import { Tabs } from "@mantine/core";
import ValuesTab from "./ValuesTab/ValuesTab";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import Card from "~/components/core/Card/Card";
import { useTranslation } from "react-i18next";

const AssetsTab = (): React.ReactNode => {
  const { t } = useTranslation();

  return (
    <Card mt="0.5rem" elevation={1}>
      <Tabs
        variant="pills"
        defaultValue="values"
        keepMounted={false}
        radius="md"
      >
        <Tabs.List grow>
          <Tabs.Tab value="values">
            <PrimaryText size="sm">{t("values")}</PrimaryText>
          </Tabs.Tab>
        </Tabs.List>
        <Tabs.Panel value="values">
          <ValuesTab />
        </Tabs.Panel>
      </Tabs>
    </Card>
  );
};

export default AssetsTab;
