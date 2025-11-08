import { Accordion, Drawer, Group, Skeleton, Stack, Text } from "@mantine/core";
import { MoveRightIcon } from "lucide-react";
import { convertNumberToCurrency } from "~/helpers/currency";
import { IAssetResponse } from "~/models/asset";
import AddValue from "./AddValue/AddValue";
import React from "react";
import { AuthContext } from "~/components/AuthProvider/AuthProvider";
import { useQuery } from "@tanstack/react-query";
import { IValueResponse } from "~/models/value";
import { AxiosResponse } from "axios";
import dayjs from "dayjs";
import ValueItems from "./ValueItems/ValueItems";

interface AssetDetailsProps {
  isOpen: boolean;
  close: () => void;
  asset: IAssetResponse | undefined;
  userCurrency: string;
}

const AssetDetails = (props: AssetDetailsProps): React.ReactNode => {
  const { request } = React.useContext<any>(AuthContext);
  const valuesQuery = useQuery({
    queryKey: ["values", props.asset?.id],
    queryFn: async (): Promise<IValueResponse[]> => {
      const res: AxiosResponse = await request({
        url: "/api/value",
        method: "GET",
        params: { assetId: props.asset?.id },
      });

      if (res.status === 200) {
        return res.data as IValueResponse[];
      }

      return [];
    },
    enabled: !!props.asset?.id && props.isOpen,
  });

  const sortedValues =
    valuesQuery.data?.sort((a, b) =>
      dayjs(b.dateTime).diff(dayjs(a.dateTime))
    ) ?? [];

  return (
    <Drawer
      opened={props.isOpen}
      onClose={props.close}
      position="right"
      size="md"
      title={
        <Text size="lg" fw={600}>
          Account Details
        </Text>
      }
      styles={{
        inner: {
          left: "0",
          right: "0",
          padding: "0 !important",
        },
      }}
    >
      {!props.asset ? (
        <Skeleton height={425} radius="lg" />
      ) : (
        <Stack>
          <Stack gap={0}>
            <Text size="xs" c="dimmed">
              Asset Name
            </Text>
            <Text size="xl" fw={600}>
              {props.asset?.name}
            </Text>
          </Stack>
          <Group justify="space-between">
            {props.asset?.purchasedDate && props.asset.purchasePrice && (
              <Stack gap={0} justify="center" align="center">
                <Text size="xs" c="dimmed">
                  Purchased on
                </Text>
                <Text size="md" fw={600}>
                  {new Date(props.asset.purchasedDate).toLocaleDateString()}
                </Text>
                <Text size="xs" c="dimmed">
                  for
                </Text>
                <Text size="md" fw={600}>
                  {convertNumberToCurrency(
                    props.asset.purchasePrice,
                    true,
                    props.userCurrency
                  )}
                </Text>
              </Stack>
            )}
            {props.asset?.purchasedDate &&
              props.asset.purchasePrice &&
              props.asset.soldDate &&
              props.asset.soldPrice && <MoveRightIcon size={32} />}
            {props.asset?.soldDate && props.asset.soldPrice && (
              <Stack gap={0} justify="center" align="center">
                <Text size="xs" c="dimmed">
                  Sold on
                </Text>
                <Text size="md" fw={600}>
                  {new Date(props.asset.soldDate).toLocaleDateString()}
                </Text>
                <Text size="xs" c="dimmed">
                  for
                </Text>
                <Text size="md" fw={600}>
                  {convertNumberToCurrency(
                    props.asset.soldPrice,
                    true,
                    props.userCurrency
                  )}
                </Text>
              </Stack>
            )}
          </Group>
          <Accordion
            variant="separated"
            defaultValue={["add-value", "values"]}
            multiple
          >
            <Accordion.Item
              value="add-value"
              bg="var(--mantine-color-content-background)"
            >
              <Accordion.Control>
                <Text>Add Value</Text>
              </Accordion.Control>
              <Accordion.Panel>
                <AddValue
                  assetId={props.asset.id}
                  currency={props.userCurrency}
                />
              </Accordion.Panel>
            </Accordion.Item>
            <Accordion.Item
              value="values"
              bg="var(--mantine-color-content-background)"
            >
              <Accordion.Control>
                <Text>Value History</Text>
              </Accordion.Control>
              <Accordion.Panel>
                <Stack gap="0.5rem">
                  {valuesQuery.isPending && (
                    <Skeleton height={20} radius="lg" />
                  )}
                  {valuesQuery.data && valuesQuery.data.length === 0 ? (
                    <Text size="sm" c="dimmed">
                      No value entries
                    </Text>
                  ) : (
                    <ValueItems
                      values={sortedValues}
                      userCurrency={props.userCurrency}
                    />
                  )}
                </Stack>
              </Accordion.Panel>
            </Accordion.Item>
          </Accordion>
        </Stack>
      )}
    </Drawer>
  );
};

export default AssetDetails;
