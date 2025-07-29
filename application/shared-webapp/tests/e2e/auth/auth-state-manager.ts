import { promises as fs } from "node:fs";
import type { BrowserContext, Page } from "@playwright/test";
import {
  getStorageStatePath,
  isAuthenticationStateValid,
  loadAuthenticationState,
  saveAuthenticationState
} from "@shared/e2e/auth/storage-state";
import type { UserRole } from "@shared/e2e/types/auth";

/**
 * Authentication state manager for handling persistence and validation
 */
export class AuthStateManager {
  private readonly workerIndex: number;
  private readonly selfContainedSystemPrefix?: string;

  constructor(workerIndex: number, selfContainedSystemPrefix?: string) {
    this.workerIndex = workerIndex;
    this.selfContainedSystemPrefix = selfContainedSystemPrefix;
  }

  /**
   * Get the storage state file path for a specific role
   * @param role User role
   * @returns Path to the storage state file
   */
  getStateFilePath(role: UserRole): string {
    return getStorageStatePath(this.workerIndex, role.toLowerCase(), this.selfContainedSystemPrefix);
  }

  /**
   * Check if authentication state exists and is valid for a role
   * @param role User role
   * @returns Promise resolving to true if auth state is valid
   */
  async hasValidAuthState(role: UserRole): Promise<boolean> {
    const filePath = this.getStateFilePath(role);
    return await isAuthenticationStateValid(filePath);
  }

  /**
   * Load authentication state for a role into a browser context
   * @param context Browser context
   * @param role User role
   * @returns Promise resolving when state is loaded
   */
  async loadAuthState(context: BrowserContext, role: UserRole): Promise<void> {
    const filePath = this.getStateFilePath(role);
    await loadAuthenticationState(context, filePath);
  }

  /**
   * Save authentication state for a role from a page
   * @param page Playwright page
   * @param role User role
   * @returns Promise resolving when state is saved
   */
  async saveAuthState(page: Page, role: UserRole): Promise<void> {
    const filePath = this.getStateFilePath(role);
    await saveAuthenticationState(page, filePath);
  }

  /**
   * Test if the authentication state is still valid by checking URL after navigation
   * @param page Playwright page with loaded auth state
   * @returns Promise resolving to true if auth is still valid
   */
  async validateAuthState(page: Page): Promise<boolean> {
    try {
      // Navigate to a protected route
      await page.goto("/admin");

      // If we get redirected to login, auth is invalid
      // If we stay on /admin (or any admin route), auth is valid
      return !page.url().includes("/login");
    } catch {
      // If any error occurs during validation, consider auth invalid
      return false;
    }
  }

  /**
   * Clear authentication state for a specific role
   * @param role User role
   * @returns Promise resolving when state is cleared
   */
  async clearAuthState(role: UserRole): Promise<void> {
    const filePath = this.getStateFilePath(role);
    try {
      await fs.unlink(filePath);
    } catch {
      // File might not exist, which is fine
    }
  }

}

/**
 * Create an AuthStateManager instance for the current worker
 * @param workerIndex Playwright worker index
 * @param selfContainedSystemPrefix Optional system prefix
 * @returns AuthStateManager instance
 */
export function createAuthStateManager(workerIndex: number, selfContainedSystemPrefix?: string): AuthStateManager {
  return new AuthStateManager(workerIndex, selfContainedSystemPrefix);
}
