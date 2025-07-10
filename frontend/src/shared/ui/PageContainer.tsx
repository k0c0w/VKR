import { Container, ContainerProps } from "@mui/material";

interface PageContainerProps extends ContainerProps {
}

const PageContainer: React.FC<PageContainerProps> = ({
    children,
    maxWidth,
    sx,
    ...props
}) => {
    return (
        <Container
            maxWidth={maxWidth?? "lg"}
            sx={{
                ...sx,
                minHeight: "100vh",
            }}
            {...props}
        >
            {children}
        </Container>
    );
};

export default PageContainer;