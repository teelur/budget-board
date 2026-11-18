import { useMediaQuery } from "@mantine/hooks";

const useIsMobile = () => useMediaQuery("(max-width: 50em)");

export default useIsMobile;
