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
import ValueItems from "./ValueItems/ValueItems";
import ValueChart from "~/components/Charts/ValueChart/ValueChart";
import Drawer from "~/components/core/Drawer/Drawer";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import StatusText from "~/components/core/Text/StatusText/StatusText";
import Accordion from "~/components/core/Accordion/Accordion";
import { useTranslation, Trans } from "react-i18next";
import { useDate } from "~/providers/DateProvider/DateProvider";

interface AssetDetailsProps {
  isOpen: boolean;
  close: () => void;
  asset: IAssetResponse | undefined;
  userCurrency: string;
}

const AssetDetails = (props: AssetDetailsProps): React.ReactNode => {
  const [chartLookbackMonths, setChartLookbackMonths] = React.useState(6);

  const { t } = useTranslation();
  const { dayjs, longDateFormat } = useDate();
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
      dayjs().subtract(chartLookbackMonths, "months"),
    ),
  );

  return (
    <Drawer
      opened={props.isOpen}
      onClose={props.close}
      position="right"
      size="md"
      title={<PrimaryText size="lg">{t("asset_details")}</PrimaryText>}
    >
      {!props.asset ? (
        <Skeleton height={425} radius="lg" />
      ) : (
        <Stack gap="1rem">
          <Stack gap={0}>
            <DimmedText size="xs">{t("asset_name")}</DimmedText>
            <PrimaryText size="xl">{props.asset?.name}</PrimaryText>
          </Stack>
          <Group justify="space-between">
            {props.asset?.purchaseDate && props.asset.purchasePrice && (
              <Stack gap={0} justify="center" align="center">
                <Trans
                  i18nKey="purchased_on_for_styled"
                  values={{
                    date: dayjs(props.asset.purchaseDate).format(
                      longDateFormat,
                    ),
                    price: convertNumberToCurrency(
                      props.asset.purchasePrice,
                      true,
                      props.userCurrency,
                    ),
                  }}
                  components={[
                    <DimmedText size="xs" key="purchased-label" />,
                    <PrimaryText size="md" key="purchased-value" />,
                  ]}
                />
              </Stack>
            )}
            {dayjs(props.asset?.purchaseDate).isValid() &&
              props.asset?.purchasePrice &&
              dayjs(props.asset?.sellDate).isValid() &&
              props.asset?.sellPrice && (
                <Stack gap={0} justify="center" align="center">
                  <MoveRightIcon size={32} />
                  <StatusText
                    amount={props.asset.sellPrice - props.asset.purchasePrice}
                    size="xs"
                  >
                    {convertNumberToCurrency(
                      props.asset.sellPrice - props.asset.purchasePrice,
                      true,
                      props.userCurrency,
                      "always",
                    )}
                  </StatusText>
                </Stack>
              )}
            {dayjs(props.asset?.sellDate).isValid() &&
              props.asset?.sellPrice && (
                <Stack gap={0} justify="center" align="center">
                  <Trans
                    i18nKey="sold_on_for_styled"
                    values={{
                      date: dayjs(props.asset.sellDate).format(longDateFormat),
                      price: convertNumberToCurrency(
                        props.asset.sellPrice,
                        true,
                        props.userCurrency,
                      ),
                    }}
                    components={[
                      <DimmedText size="xs" key="sold-label" />,
                      <PrimaryText size="md" key="sold-value" />,
                    ]}
                  />
                </Stack>
              )}
          </Group>
          <Accordion
            defaultValue={["add-value", "chart", "values"]}
            elevation={1}
          >
            <MantineAccordion.Item value="add-value">
              <MantineAccordion.Control>
                <PrimaryText>{t("add_value")}</PrimaryText>
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
                <PrimaryText>{t("value_trends")}</PrimaryText>
              </MantineAccordion.Control>
              <MantineAccordion.Panel>
                <Group>
                  <Button
                    variant={chartLookbackMonths === 3 ? "filled" : "outline"}
                    size="xs"
                    onClick={() => setChartLookbackMonths(3)}
                  >
                    {t("3_months")}
                  </Button>
                  <Button
                    variant={chartLookbackMonths === 6 ? "filled" : "outline"}
                    size="xs"
                    onClick={() => setChartLookbackMonths(6)}
                  >
                    {t("6_months")}
                  </Button>
                  <Button
                    variant={chartLookbackMonths === 12 ? "filled" : "outline"}
                    size="xs"
                    onClick={() => setChartLookbackMonths(12)}
                  >
                    {t("12_months")}
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
                <PrimaryText>{t("value_history")}</PrimaryText>
              </MantineAccordion.Control>
              <MantineAccordion.Panel>
                <Stack gap="0.5rem">
                  {valuesQuery.isPending && (
                    <Skeleton height={20} radius="lg" />
                  )}
                  {sortedValues.length === 0 ? (
                    <Group justify="center">
                      <DimmedText size="sm">{t("no_value_entries")}</DimmedText>
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
                <PrimaryText>{t("deleted_values")}</PrimaryText>
              </MantineAccordion.Control>
              <MantineAccordion.Panel>
                <Stack gap="0.5rem">
                  {valuesQuery.isPending && (
                    <Skeleton height={20} radius="lg" />
                  )}
                  {sortedDeletedValues.length === 0 ? (
                    <DimmedText size="sm">{t("no_deleted_values")}</DimmedText>
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
