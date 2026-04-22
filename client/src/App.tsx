import { Navigate, Route, Routes } from "react-router-dom";

import PortfolioLayout from "./components/portfolio-layout";
import Wallet from "./pages/Wallet";

export default function App() {
  return (
    <Routes>
      <Route path="/" element={<Navigate replace to="/wallet" />} />
      <Route element={<PortfolioLayout />}>
        <Route path="/wallet" element={<Wallet />} />
      </Route>
    </Routes>
  );
}
