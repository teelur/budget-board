import { MantineColorScheme, useMantineColorScheme } from "@mantine/core";
import { useTranslation } from "react-i18next";
import Select from "~/components/core/Select/Select/Select";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";

const DarkModeToggle = () => {
  const { t } = useTranslation();

  const darkModeOptions = [
    { label: t("auto"), value: "auto" },
    { label: t("light"), value: "light" },
    { label: t("dark"), value: "dark" },
  ];
  const { colorScheme, setColorScheme } = useMantineColorScheme();

  return (
    <Select
      data={darkModeOptions}
      label={<PrimaryText size="sm">{t("appearance_mode")}</PrimaryText>}
      value={colorScheme}
      onChange={(value) => setColorScheme(value as MantineColorScheme)}
      elevation={0}
    />
  );
};

export default DarkModeToggle;
