import React from 'react';
import { Link } from 'react-router';

import "./home.css";

import { MacbookScroll } from "@/components/ui/macbook-scroll";
import FloatingDockComponent from "../components/floating-dock-component";
import NoiseBackgroundIcon from "../components/noise-background-logo";

import icon from "../../public/icon.png";

export default function Home() {
  return (
    <div className="relative w-full min-h-screen overflow-hidden bg-white dark:bg-[#0B0B0F]">

      {/* BACKGROUND LAYER */}
      <div className="absolute inset-0">
        <div className="relative h-full w-full bg-slate-950
          [&>div]:absolute
          [&>div]:bottom-0
          [&>div]:right-[-20%]
          [&>div]:top-[-10%]
          [&>div]:h-[500px]
          [&>div]:w-[500px]
          [&>div]:rounded-full
          [&>div]:bg-[radial-gradient(circle_farthest-side,rgba(0,245,200,0.18),rgba(255,255,255,0))]">
          
          <div></div>
        </div>

        {/* OPTIONAL: keep your noise overlay */}
        <NoiseBackgroundIcon />
      </div>

      {/* CONTENT LAYER */}
      <div className="relative z-10">

        <div className="flex justify-center py-4">
          <FloatingDockComponent />
        </div>

        <div className="absolute top-12 right-10 mr-10 flex flex-col gap-8">
          <Link to="/login">
            <button className="shadow-[inset_0_0_0_2px_#616467] px-8 py-4 rounded-full font-bold text-white tracking-widest uppercase transform hover:scale-110 hover:bg-[#616467] transition-colors duration-200 hover:shadow-2xl hover:shadow-white/[0.1]">
              Log in
            </button>
          </Link>

          <button className="shadow-[inset_0_0_0_2px_#616467] px-8 py-4 rounded-full font-bold text-white tracking-widest uppercase transform hover:scale-110 hover:bg-[#616467] transition-colors duration-200 hover:shadow-2xl hover:shadow-white/[0.1]">
            Sign up
          </button>
        </div>

        <div className="w-full overflow-hidden">
          <MacbookScroll
            title={""}
            badge={<img src={icon} className="w-20 h-30" alt="icon" />}
            src={`/linear.webp`}
            showGradient={false}
          />
        </div>

      </div>
    </div>
  );
}