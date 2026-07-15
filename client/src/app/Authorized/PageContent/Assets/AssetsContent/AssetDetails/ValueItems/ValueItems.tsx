import { Group, Pagination, Stack } from "@mantine/core";
import React from "react";
import { IValueResponse } from "~/models/value";
import ValueItem from "./ValueItem/ValueItem";
import { useUserSettings } from "~/providers/UserSettingsProvider/UserSettingsProvider";

interface ValueItemsProps {
  values: IValueResponse[];
}

const ValueItems = (props: ValueItemsProps): React.ReactNode => {
  const itemsPerPage = 10;

  const { preferredCurrency } = useUserSettings();

  const [page, setPage] = React.useState(1);

  React.useEffect(() => {
    setPage(1);
  }, [props.values]);

  return (
    <Stack gap="0.5rem">
      {props.values
        .slice((page - 1) * itemsPerPage, page * itemsPerPage)
        .map((value) => (
          <ValueItem
            key={value.id}
            value={value}
            userCurrency={preferredCurrency}
          />
        ))}
      <Group justify="center">
        <Pagination
          total={Math.ceil(props.values.length / itemsPerPage)}
          value={page}
          onChange={setPage}
        />
      </Group>
    </Stack>
  );
};

export default ValueItems;
