import { Pipe, PipeTransform } from '@angular/core';
import { environment } from '../../../environments/environment';

const LOCAL_PLACEHOLDER = '/assets/images/product-placeholder.svg';

const LOCAL_PRODUCT_ASSETS: Record<string, string> = {
  'premium-smartphone': '/assets/products/premium-smartphone.svg',
  'budget-smartphone': '/assets/products/budget-smartphone.svg',
  'gaming-laptop': '/assets/products/gaming-laptop.svg',
  'ultrabook': '/assets/products/ultrabook.svg',
  'wireless-earbuds': '/assets/products/wireless-earbuds.svg',
  'classic-tshirt': '/assets/products/classic-tshirt.svg',
  'smart-led-bulb': '/assets/products/smart-led-bulb.svg',
  'garden-tool-set': '/assets/products/garden-tool-set.svg',
};

const UNRELIABLE_HOSTS = ['via.placeholder.com', 'placeholder.com'];

@Pipe({
  name: 'productImage',
  standalone: true
})
export class ProductImagePipe implements PipeTransform {
  private readonly apiBaseUrl: string;

  constructor() {
    const url = environment.apiBaseUrl;
    this.apiBaseUrl = url.replace(/\/api$/, '');
  }

  transform(imageUrl: string | null | undefined, productSlug?: string | null): string {
    if (productSlug && LOCAL_PRODUCT_ASSETS[productSlug]) {
      return LOCAL_PRODUCT_ASSETS[productSlug];
    }

    if (!imageUrl || !imageUrl.trim()) {
      return LOCAL_PLACEHOLDER;
    }

    if (imageUrl.startsWith('http://') || imageUrl.startsWith('https://') || imageUrl.startsWith('data:')) {
      if (UNRELIABLE_HOSTS.some(host => imageUrl.includes(host))) {
        return LOCAL_PLACEHOLDER;
      }
      return imageUrl;
    }

    const normalizedPath = imageUrl.startsWith('/') ? imageUrl : `/${imageUrl}`;
    return `${this.apiBaseUrl}${normalizedPath}`;
  }
}
