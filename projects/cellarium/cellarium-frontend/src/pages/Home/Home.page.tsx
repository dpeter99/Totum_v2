import { Link } from "react-router-dom";


export const Home = () => {
    
  return (
    <div>
      Home page
        <Link to={'/app/shoppingList'}> Shopping lists </Link>
    </div>
  );
};