import { Group, Skeleton, Stack } from "@mantine/core";
import {
  Button,
  AmountText,
  HeadingText,
  BodyText,
  CollapsibleCard,
} from "@teelur/budget-board-ui";
import { usePrivacyMode } from "~/providers/PrivacyModeProvider/PrivacyModeProvider";
import { MoveRightIcon } from "lucide-react";
import { SignDisplay } from "~/helpers/currency";
import { IAssetResponse } from "~/models/asset";
import AddValue from "./AddValue/AddValue";
import React from "react";
import ValueItems from "./ValueItems/ValueItems";
import ValueChart from "~/components/Charts/ValueChart/ValueChart";
import Drawer from "~/components/core/Drawer/Drawer";
import { useTranslation, Trans } from "react-i18next";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import { useValuesQuery } from "~/hooks/queries/useValuesQuery";
import { useSensitiveAmountFormatter } from "~/hooks/useSensitiveAmountFormatter";
import { useUserSettings } from "~/providers/UserSettingsProvider/UserSettingsProvider";

interface AssetDetailsProps {
  isOpen: boolean;
  close: () => void;
  asset: IAssetResponse | undefined;
}

const AssetDetails = (props: AssetDetailsProps): React.ReactNode => {
  const [chartLookbackMonths, setChartLookbackMonths] = React.useState(6);

  const { t } = useTranslation();
  const { dayjs, longDateFormat, intlLocale } = useLocale();
  const { preferredCurrency, decimalPlaces } = useUserSettings();
  const { isPrivacyModeEnabled } = usePrivacyMode();
  const formatAmount = useSensitiveAmountFormatter();
  const valuesQuery = useValuesQuery({
    assetIds: props.asset ? [props.asset.id] : [],
    enabled: !!props.asset?.id && props.isOpen,
  });

  const sortedValues =
    valuesQuery.data?.sort((a, b) => dayjs(b.date).diff(dayjs(a.date))) ?? [];

  const valuesForChart = sortedValues.filter((value) =>
    dayjs(value.date).isAfter(dayjs().subtract(chartLookbackMonths, "months")),
  );

  const formatSensitiveAmount = (
    amount: number,
    signDisplay = SignDisplay.Auto,
  ): string => formatAmount(amount, signDisplay);

  return (
    <Drawer
      opened={props.isOpen}
      onClose={props.close}
      position="right"
      size="md"
      title={<HeadingText level={4}>{t("asset_details")}</HeadingText>}
    >
      {!props.asset ? (
        <Skeleton height={425} radius="lg" />
      ) : (
        <Stack gap="0.5rem">
          <Stack gap={0}>
            <BodyText tone="secondary" size="xs">
              {t("asset_name")}
            </BodyText>
            <BodyText tone="primary" size="lg" fw={500}>
              {props.asset?.name}
            </BodyText>
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
                    price: formatSensitiveAmount(props.asset.purchasePrice),
                  }}
                  components={[
                    <BodyText
                      tone="secondary"
                      size="xs"
                      key="purchased-label"
                    />,
                    <BodyText
                      tone="primary"
                      size="md"
                      fw={500}
                      key="purchased-value"
                    />,
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
                  <AmountText
                    amount={props.asset.sellPrice - props.asset.purchasePrice}
                    size="xs"
                    isSensitive={isPrivacyModeEnabled}
                    locale={intlLocale}
                    currency={preferredCurrency}
                    decimalPlaces={decimalPlaces}
                    signDisplay={SignDisplay.Always}
                  />
                </Stack>
              )}
            {dayjs(props.asset?.sellDate).isValid() &&
              props.asset?.sellPrice && (
                <Stack gap={0} justify="center" align="center">
                  <Trans
                    i18nKey="sold_on_for_styled"
                    values={{
                      date: dayjs(props.asset.sellDate).format(longDateFormat),
                      price: formatSensitiveAmount(props.asset.sellPrice),
                    }}
                    components={[
                      <BodyText tone="secondary" size="xs" key="sold-label" />,
                      <BodyText
                        tone="primary"
                        size="md"
                        fw={500}
                        key="sold-value"
                      />,
                    ]}
                  />
                </Stack>
              )}
          </Group>
          <Stack gap="0.5rem">
            <CollapsibleCard
              title={
                <HeadingText tone="heading" level={5}>
                  {t("add_value")}
                </HeadingText>
              }
            >
              <AddValue assetId={props.asset.id} />
            </CollapsibleCard>
            <CollapsibleCard
              title={
                <HeadingText tone="heading" level={5}>
                  {t("value_trends")}
                </HeadingText>
              }
            >
              <Group gap="0.5rem">
                <Button
                  variant="filled"
                  color="primary"
                  size="compact-xs"
                  selected={chartLookbackMonths === 3}
                  onClick={() => setChartLookbackMonths(3)}
                >
                  {t("3_months")}
                </Button>
                <Button
                  variant="filled"
                  color="primary"
                  size="compact-xs"
                  selected={chartLookbackMonths === 6}
                  onClick={() => setChartLookbackMonths(6)}
                >
                  {t("6_months")}
                </Button>
                <Button
                  variant="filled"
                  color="primary"
                  size="compact-xs"
                  selected={chartLookbackMonths === 12}
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
            </CollapsibleCard>
            <CollapsibleCard
              title={
                <HeadingText tone="heading" level={5}>
                  {t("value_history")}
                </HeadingText>
              }
            >
              <Stack gap="0.5rem">
                {valuesQuery.isPending && <Skeleton height={20} radius="lg" />}
                {sortedValues.length === 0 ? (
                  <Group justify="center">
                    <BodyText tone="muted" size="sm">
                      {t("no_value_entries")}
                    </BodyText>
                  </Group>
                ) : (
                  <ValueItems values={sortedValues} />
                )}
              </Stack>
            </CollapsibleCard>
          </Stack>
        </Stack>
      )}
    </Drawer>
  );
};

export default AssetDetails;
