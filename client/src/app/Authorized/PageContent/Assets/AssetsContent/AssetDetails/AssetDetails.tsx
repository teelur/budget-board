import {
  Accordion as MantineAccordion,
  Button,
  Group,
  Skeleton,
  Stack,
} from "@mantine/core";
import { MoveRightIcon } from "lucide-react";
import { convertNumberToCurrency } from "~/helpers/currency";
import { IAssetResponse } from "~/models/asset";
import AddValue from "./AddValue/AddValue";
import React from "react";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { useQuery } from "@tanstack/react-query";
import { IValueResponse } from "~/models/value";
import { AxiosResponse } from "axios";
import dayjs from "dayjs";
import ValueItems from "./ValueItems/ValueItems";
import ValueChart from "~/components/Charts/ValueChart/ValueChart";
import Drawer from "~/components/Drawer/Drawer";
import PrimaryText from "~/components/Text/PrimaryText/PrimaryText";
import DimmedText from "~/components/Text/DimmedText/DimmedText";
import StatusText from "~/components/Text/StatusText/StatusText";
import Accordion from "~/components/Accordion/Accordion";

interface AssetDetailsProps {
  isOpen: boolean;
  close: () => void;
  asset: IAssetResponse | undefined;
  userCurrency: string;
}

const AssetDetails = (props: AssetDetailsProps): React.ReactNode => {
  const [chartLookbackMonths, setChartLookbackMonths] = React.useState(6);

  const { request } = useAuth();
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
    valuesQuery.data
      ?.filter((value) => value.deleted === null)
      .sort((a, b) => dayjs(b.dateTime).diff(dayjs(a.dateTime))) ?? [];

  const sortedDeletedValues =
    valuesQuery.data
      ?.filter((value) => value.deleted !== null)
      .sort((a, b) => dayjs(b.dateTime).diff(dayjs(a.dateTime))) ?? [];

  const valuesForChart = sortedValues.filter((value) =>
    dayjs(value.dateTime).isAfter(
      dayjs().subtract(chartLookbackMonths, "months")
    )
  );

  return (
    <Drawer
      opened={props.isOpen}
      onClose={props.close}
      position="right"
      size="md"
      title={<PrimaryText size="lg">Asset Details</PrimaryText>}
    >
      {!props.asset ? (
        <Skeleton height={425} radius="lg" />
      ) : (
        <Stack gap="1rem">
          <Stack gap={0}>
            <DimmedText size="xs">Asset Name</DimmedText>
            <PrimaryText size="xl">{props.asset?.name}</PrimaryText>
          </Stack>
          <Group justify="space-between">
            {props.asset?.purchaseDate && props.asset.purchasePrice && (
              <Stack gap={0} justify="center" align="center">
                <DimmedText size="xs">Purchased on</DimmedText>
                <PrimaryText size="md" fw={600}>
                  {new Date(props.asset.purchaseDate).toLocaleDateString()}
                </PrimaryText>
                <DimmedText size="xs">for</DimmedText>
                <PrimaryText size="md" fw={600}>
                  {convertNumberToCurrency(
                    props.asset.purchasePrice,
                    true,
                    props.userCurrency
                  )}
                </PrimaryText>
              </Stack>
            )}
            {props.asset?.purchaseDate &&
              props.asset.purchasePrice &&
              props.asset.sellDate &&
              props.asset.sellPrice && (
                <Stack gap={0} justify="center" align="center">
                  <MoveRightIcon size={32} />
                  <StatusText
                    amount={props.asset.sellPrice - props.asset.purchasePrice}
                    size="xs"
                  >
                    {props.asset.sellPrice - props.asset.purchasePrice >= 0
                      ? "+"
                      : ""}
                    {convertNumberToCurrency(
                      props.asset.sellPrice - props.asset.purchasePrice,
                      true,
                      props.userCurrency
                    )}
                  </StatusText>
                </Stack>
              )}
            {props.asset?.sellDate && props.asset.sellPrice && (
              <Stack gap={0} justify="center" align="center">
                <DimmedText size="xs">Sold on</DimmedText>
                <PrimaryText size="md" fw={600}>
                  {new Date(props.asset.sellDate).toLocaleDateString()}
                </PrimaryText>
                <DimmedText size="xs">for</DimmedText>
                <PrimaryText size="md" fw={600}>
                  {convertNumberToCurrency(
                    props.asset.sellPrice,
                    true,
                    props.userCurrency
                  )}
                </PrimaryText>
              </Stack>
            )}
          </Group>
          <Accordion
            defaultValue={["add-value", "chart", "values"]}
            elevation={1}
          >
            <MantineAccordion.Item value="add-value">
              <MantineAccordion.Control>
                <PrimaryText>Add Value</PrimaryText>
              </MantineAccordion.Control>
              <MantineAccordion.Panel>
                <AddValue
                  assetId={props.asset.id}
                  currency={props.userCurrency}
                />
              </MantineAccordion.Panel>
            </MantineAccordion.Item>
            <MantineAccordion.Item value="chart">
              <MantineAccordion.Control>
                <PrimaryText>Value Trends</PrimaryText>
              </MantineAccordion.Control>
              <MantineAccordion.Panel>
                <Group>
                  <Button
                    variant={chartLookbackMonths === 3 ? "filled" : "outline"}
                    size="xs"
                    onClick={() => setChartLookbackMonths(3)}
                  >
                    3 months
                  </Button>
                  <Button
                    variant={chartLookbackMonths === 6 ? "filled" : "outline"}
                    size="xs"
                    onClick={() => setChartLookbackMonths(6)}
                  >
                    6 months
                  </Button>
                  <Button
                    variant={chartLookbackMonths === 12 ? "filled" : "outline"}
                    size="xs"
                    onClick={() => setChartLookbackMonths(12)}
                  >
                    12 months
                  </Button>
                </Group>
                <ValueChart
                  items={[
                    {
                      id: props.asset.id,
                      name: props.asset.name,
                    },
                  ]}
                  values={valuesForChart.map((value) => ({
                    ...value,
                    parentId: value.assetID || "",
                  }))}
                  dateRange={[
                    dayjs().subtract(chartLookbackMonths, "months").toString(),
                    dayjs().toString(),
                  ]}
                />
              </MantineAccordion.Panel>
            </MantineAccordion.Item>
            <MantineAccordion.Item value="values">
              <MantineAccordion.Control>
                <PrimaryText>Value History</PrimaryText>
              </MantineAccordion.Control>
              <MantineAccordion.Panel>
                <Stack gap="0.5rem">
                  {valuesQuery.isPending && (
                    <Skeleton height={20} radius="lg" />
                  )}
                  {sortedValues.length === 0 ? (
                    <Group justify="center">
                      <DimmedText size="sm">No value entries.</DimmedText>
                    </Group>
                  ) : (
                    <ValueItems
                      values={sortedValues}
                      userCurrency={props.userCurrency}
                    />
                  )}
                </Stack>
              </MantineAccordion.Panel>
            </MantineAccordion.Item>
            <MantineAccordion.Item value="deleted-values">
              <MantineAccordion.Control>
                <PrimaryText>Deleted Values</PrimaryText>
              </MantineAccordion.Control>
              <MantineAccordion.Panel>
                <Stack gap="0.5rem">
                  {valuesQuery.isPending && (
                    <Skeleton height={20} radius="lg" />
                  )}
                  {sortedDeletedValues.length === 0 ? (
                    <DimmedText size="sm">No deleted values.</DimmedText>
                  ) : (
                    <ValueItems
                      values={sortedDeletedValues}
                      userCurrency={props.userCurrency}
                    />
                  )}
                </Stack>
              </MantineAccordion.Panel>
            </MantineAccordion.Item>
          </Accordion>
        </Stack>
      )}
    </Drawer>
  );
};

export default AssetDetails;
