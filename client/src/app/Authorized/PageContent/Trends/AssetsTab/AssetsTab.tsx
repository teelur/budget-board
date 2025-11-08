import { Card, Tabs, Text } from "@mantine/core";
import ValuesTab from "./ValuesTab/ValuesTab";

const AssetsTab = (): React.ReactNode => {
  return (
    <Card radius="md" withBorder mt="0.5rem" p="0.5rem">
      <Tabs
        variant="pills"
        defaultValue="values"
        keepMounted={false}
        radius="md"
      >
        <Tabs.List grow>
          <Tabs.Tab value="values">
            <Text fw={600} size="sm">
              Values
            </Text>
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
