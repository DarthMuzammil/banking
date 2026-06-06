import type { Metadata } from "next";
import { Inter } from "next/font/google";
import { Providers } from "./providers";
import "./globals.css";

const inter = Inter({
  subsets: ["latin"],
  variable: "--font-inter",
  display: "swap",
});

export const metadata: Metadata = {
  title: "Banking",
  description: "A calm, focused banking workspace",
};

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="en" className={`${inter.variable} dark h-full`}>
      <body className="min-h-full bg-surface text-foreground antialiased">
        <Providers>{children}</Providers>
      </body>
    </html>
  );
}
