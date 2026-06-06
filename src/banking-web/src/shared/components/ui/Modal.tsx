"use client";

import { useEffect, useRef } from "react";
import { cn } from "@/shared/lib/cn";
import { Button } from "./Button";

interface ModalProps {
  open: boolean;
  onClose: () => void;
  title: string;
  description?: string;
  children: React.ReactNode;
  footer?: React.ReactNode;
}

export function Modal({ open, onClose, title, description, children, footer }: ModalProps) {
  const dialogRef = useRef<HTMLDialogElement>(null);

  useEffect(() => {
    const dialog = dialogRef.current;
    if (!dialog) return;
    if (open && !dialog.open) dialog.showModal();
    if (!open && dialog.open) dialog.close();
  }, [open]);

  if (!open) return null;

  return (
    <dialog
      ref={dialogRef}
      className={cn(
        "fixed inset-0 z-50 m-auto w-full max-w-md rounded-lg border border-border bg-surface-tertiary p-0 shadow-[0_24px_64px_rgba(0,0,0,0.5)] backdrop:bg-black/60",
        "open:animate-fade-in",
      )}
      onClose={onClose}
      onClick={(e) => { if (e.target === dialogRef.current) onClose(); }}
    >
      <div className="border-b border-border px-6 py-4">
        <h2 className="text-base font-semibold text-foreground">{title}</h2>
        {description ? <p className="mt-1 text-sm text-muted">{description}</p> : null}
      </div>
      <div className="px-6 py-5">{children}</div>
      {footer ? (
        <div className="flex justify-end gap-2 border-t border-border px-6 py-4">{footer}</div>
      ) : (
        <div className="flex justify-end border-t border-border px-6 py-4">
          <Button variant="secondary" onClick={onClose}>Close</Button>
        </div>
      )}
    </dialog>
  );
}
