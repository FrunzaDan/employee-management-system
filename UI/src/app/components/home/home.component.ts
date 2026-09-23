import { Component } from '@angular/core';
import { EmployeeListComponent } from '../employee-list/employee-list.component';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrl: './home.component.css',
  imports: [EmployeeListComponent],
})
export class HomeComponent {}
