export interface Category {
  id: string;
  name: string;
  slug: string;
  description: string | null;
  displayOrder: number;
  children?: Category[];
}

export interface CategorySimple {
  id: string;
  name: string;
}
