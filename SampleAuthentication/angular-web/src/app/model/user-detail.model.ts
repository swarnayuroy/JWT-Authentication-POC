export interface UserDetail {
  id: string;
  name: string;
  email: string;
  role: string;
  isVerified?: boolean;
  password: string;
}
