import { Button, Group, TextInput } from "@mantine/core";

interface RuleItemProps {
  regexRule: string;
}

const RuleItem = (props: RuleItemProps): React.ReactNode => {
  return (
    <Group>
      <TextInput placeholder="Rule Name" />
      <TextInput placeholder="Regex Rule" />
      <Button>Remove</Button>
    </Group>
  );
};

export default RuleItem;
