import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterOutlet, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs/operators';

interface OperationCard {
  type: 'create' | 'update' | 'delete' | 'get';
  label: string;
  icon: string;
  description: string;
}

@Component({
  selector: 'app-root',
  imports: [CommonModule, RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App implements OnInit {
  title = 'REST vs GraphQL Client';
  connectionStatus: { success: boolean; message: string } | null = null;
  isHomePage = true;

  operations: OperationCard[] = [
    {
      type: 'create',
      label: 'Bulk Create',
      icon: '✅',
      description: 'Create multiple orders in one request'
    },
    {
      type: 'update',
      label: 'Bulk Update',
      icon: '🔄',
      description: 'Update multiple orders simultaneously'
    },
    {
      type: 'delete',
      label: 'Bulk Delete',
      icon: '🗑️',
      description: 'Delete multiple orders at once'
    },
    {
      type: 'get',
      label: 'Bulk Get',
      icon: '📋',
      description: 'Retrieve all orders with full details'
    }
  ];

  constructor(private router: Router) {}

  ngOnInit(): void {
    // Set initial state based on current URL
    this.isHomePage = this.router.url === '/' || this.router.url === '';
    
    // Track route changes to show/hide home page
    this.router.events
      .pipe(filter(event => event instanceof NavigationEnd))
      .subscribe((event: any) => {
        this.isHomePage = event.url === '/' || event.url === '';
      });
  }

  async testConnection(): Promise<void> {
    this.connectionStatus = null;
    try {
      const response = await fetch('http://localhost:5072/api/customers');
      if (response.ok) {
        const data = await response.json();
        this.connectionStatus = { success: true, message: `✅ Connected! Found ${data.length} customers.` };
      } else {
        this.connectionStatus = { success: false, message: `❌ Backend responded with status: ${response.status}` };
      }
    } catch {
      this.connectionStatus = { success: false, message: '❌ Cannot connect. Make sure the .NET API is running on port 5072.' };
    }
  }

  navigateToOperation(type: string): void {
    this.router.navigate(['/operation', type]);
  }
}
