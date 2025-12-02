import { Tabs } from "@mantine/core";
import ValuesTab from "./ValuesTab/ValuesTab";
import PrimaryText from "~/components/Text/PrimaryText/PrimaryText";
import Card from "~/components/Card/Card";

const AssetsTab = (): React.ReactNode => {
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
            <PrimaryText size="sm">Values</PrimaryText>
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
