import { Link } from 'react-router-dom';
import { Button } from '@mui/material';
import { Link as RouterLink } from 'react-router-dom'; // Optional: alias to avoid naming conflicts

function MyComponent() {
  return (
    <Button
      component={RouterLink} // Use Link from react-router-dom
      to="/destination" // Required for react-router-dom Link
      variant="contained"
      color="primary"
    >
      Go to Destination
    </Button>
  );
}

export default MyComponent;