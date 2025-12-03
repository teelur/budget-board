import { Flex, Grid } from "@mantine/core";
import { ArrowRightIcon } from "lucide-react";
import React from "react";
import { IAccountItem } from "../AccountMapping";
import { useField } from "@mantine/form";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import Select from "~/components/core/Select/Select/Select";

interface AccountMappingItemProps {
  accountName: string;
  accountId: string;
  accounts: IAccountItem[];
  onAccountChange: (accountName: string, accountId: string) => void;
}

const AccountMappingItem = (
  props: AccountMappingItemProps
): React.ReactNode => {
  const accountMappingField = useField<string | null>({
    initialValue:
      props.accounts.find((a) => a.value === props.accountId)?.value ?? null,
  });

  React.useEffect(() => {
    props.onAccountChange(
      props.accountName,
      accountMappingField.getValue() ?? ""
    );
  }, [accountMappingField.getValue()]);

  return (
    <Grid w="100%" align="center">
      <Grid.Col span={4}>
        <PrimaryText size="md">{props.accountName}</PrimaryText>
      </Grid.Col>
      <Grid.Col span={3}>
        <Flex justify="center" align="center">
          <ArrowRightIcon size={16} />
        </Flex>
      </Grid.Col>
      <Grid.Col span={5}>
        <Select
          data={props.accounts}
          {...accountMappingField.getInputProps()}
          clearable
          placeholder="Select account"
          elevation={0}
        />
      </Grid.Col>
    </Grid>
  );
};

export default AccountMappingItem;
